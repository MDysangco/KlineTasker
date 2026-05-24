using Microsoft.Extensions.Configuration;
using Zyprix.Data.Interfaces;
using Zyprix.Data.Repositories;
using Zyprix.Models;
using Zyprix.Services;
using Zyprix.Services.Interfaces;

namespace TrenchLooter.CronTasks
{
    public class BackFillKlines
    {
        public static async Task<bool> Run(IConfiguration config, CancellationToken cancellationToken)
        {
            try
            {
                string token = Utils.JwtFactory.CreateInternalServiceToken(config, "tasker", 60);
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("Unable to generate token for internal service.");
                    return false;
                }

                BinanceClient binanceClient = new BinanceClient();
                ZypryxClient zypryxClient = new ZypryxClient(token);

                List<Coin>? coins = await zypryxClient.GetAllCoins();
                if (coins == null || !coins.Any())
                {
                    Console.WriteLine("No active coins found in Zypryx.");
                    return false;
                }

                if (!await binanceClient.CheckConnection())
                {
                    Console.WriteLine("Unable to connect to Binance.");
                    return false;
                }

                foreach (Coin coin in coins)
                {
                    try
                    {
                        if (!coin.BinanceListingDate.HasValue) continue;

                        List<Kline> klines = new List<Kline>();
                        Kline? kline = await zypryxClient.GetEarliestKline(coin.Id, KlineInterval.OneHour);

                        if (kline == null || !kline.KlineOpenTime.HasValue)
                        {
                            klines.AddRange(await binanceClient.GetKlines(coin, KlineInterval.OneHour, 1000));
                        }
                        else
                        {
                            DateTime earliestReadingDate = DateTimeOffset.FromUnixTimeMilliseconds(kline.KlineOpenTime.Value).UtcDateTime;
                            DateTime listingDate = DateTimeOffset.FromUnixTimeMilliseconds(coin.BinanceListingDate.Value).UtcDateTime;
                            DateTime sevenYearsAgo = DateTime.UtcNow.AddYears(-7).AddDays(-1);
                            DateTime stopBoundary = (listingDate > sevenYearsAgo) ? listingDate : sevenYearsAgo;

                            if (earliestReadingDate > stopBoundary)
                            {
                                List<Kline> klinesAPI = await binanceClient.GetKlines(coin, KlineInterval.OneHour, 1000, earliestReadingDate, null);

                                if (klinesAPI != null && klinesAPI.Count > 0)
                                {
                                    klines.AddRange(klinesAPI);
                                }
                            }
                        }

                        if (klines.Any())
                        {

                            await zypryxClient.InsertKlines(klines);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unable to fetch klines for {coin.Ticker}: {ex.ToString()}");
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
