using Microsoft.Extensions.Configuration;
using Zyprix.Data.Interfaces;
using Zyprix.Data.Repositories;
using Zyprix.Models;
using Zyprix.Services;
using Zyprix.Services.Interfaces;

namespace TrenchLooter.CronTasks
{
    public class UpdateKlines
    {
        /// <summary>
        /// This task will pick up all coins and update with the 
        /// latest one hour candles.
        /// </summary>
        /// <returns></returns>
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

                List<Coin>? coins = await zypryxClient.GetActiveCoins();
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
                        Kline kline = await zypryxClient.GetLatestKline(coin.Id, KlineInterval.OneHour);
                        List<Kline> klines = new List<Kline>();

                        if (kline == null || string.IsNullOrEmpty(kline?.KlineOpenTime))
                        {
                            klines.AddRange(await binanceClient.GetKlines(coin, KlineInterval.OneHour, 1000));
                        }
                        else
                        {

                            DateTime dt = DateTime.UtcNow;
                            DateTime roundedDown = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);

                            long startDateLong = long.Parse(kline.KlineOpenTime);
                            DateTime startDate = DateTimeOffset.FromUnixTimeMilliseconds(startDateLong).UtcDateTime;

                            if (roundedDown == startDate)
                            {
                                continue;
                            }

                            klines.AddRange(await binanceClient.GetKlines(coin, KlineInterval.OneHour, 1000, null, startDate));
                        }

                        if (klines.Any())
                        {
                            await zypryxClient.InsertKlines(klines);
                            await Task.Delay(1000);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unable to fetch klines for {coin.Ticker}: {ex.ToString()}");
                    }
                }

                return true;
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

    }
}
