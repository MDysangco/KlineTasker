using Microsoft.Extensions.Configuration;
using Zyprix.Data.Interfaces;
using Zyprix.Data.Repositories;
using Zyprix.Models;
using Zyprix.Services;
using Zyprix.Services.Interfaces;

namespace TrenchLooter.CronTasks
{
    public class UpdateCoins
    {
        public static async Task<bool> Run(CancellationToken cancellationToken)
        {
            try
            {
                BinanceClient binanceClient = new BinanceClient();
                ZypryxClient zypryxClient = new ZypryxClient();

                List<Coin>? coins = await zypryxClient.GetActiveCoins();
                if (coins == null || !coins.Any())
                {
                    Console.WriteLine("No active coins found in Zypryx.");
                    return false;
                }

                coins = coins.Where(coin => !coin.BinanceListingDate.HasValue).ToList();
                if (coins == null || !coins.Any())
                {
                    Console.WriteLine("No coins without Binance listing date found.");
                    return false;
                }

                if(!await binanceClient.CheckConnection())
                {
                    Console.WriteLine("Unable to connect to Binance.");
                    return false;
                }

                foreach (Coin coin in coins)
                {
                    DateTime startingEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                    List<Kline> klines = await binanceClient.GetKlines(coin, KlineInterval.OneHour, 1, null, startingEpoch);

                    if (klines.Any())
                    {
                        Kline kline = klines.First();

                        if (long.TryParse(kline.KlineOpenTime, out long klineOpenTime))
                        {

                            coin.BinanceListingDate = klineOpenTime;
                            coin.Active = true;

                            bool updated = await zypryxClient.UpdateCoin(coin);
                            if (updated)
                            {
                                return true;
                            }
                        }
                        return false;
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
