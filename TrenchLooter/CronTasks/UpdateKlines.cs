using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrenchLooter.Models;

namespace TrenchLooter.CronTasks
{
    public class UpdateKlines
    {
        /// <summary>
        /// This task will pick up all coins and update with the 
        /// latest one hour candles.
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> Run(CancellationToken cancellationToken)
         {
            try
            {
                BinanceClient client = new BinanceClient();

                if (await client.CheckConnection())
                {
                    List<Coin> coinList = StoredProcedures.GetActiveCoins();
                    List<Kline.APITickerKline> klines = new List<Kline.APITickerKline>();

                    foreach (Coin coin in coinList)
                    {
                        try
                        {
                            Kline.TickerKline kline = await StoredProcedures.GetLatestRecordedKline(coin, Kline.KlineInterval.OneHour);
                            
                            if (kline == null || string.IsNullOrEmpty(kline?.KlineOpenTime))
                            {
                                klines.AddRange(await client.GetKlines(coin, Kline.KlineInterval.OneHour, 1000));
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

                                klines.AddRange(await client.GetKlines(coin, Kline.KlineInterval.OneHour, 1000, null, startDate));
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unable to fetch klines for {coin.Ticker}: {ex.ToString()}");
                        }
                    }

                    bool inserted = await StoredProcedures.Insertklines(klines);
                    return inserted;
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
