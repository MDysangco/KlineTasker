using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TrenchLooter.Models;

namespace TrenchLooter.CronTasks
{
    public class BackFillKlines
    {
        public static async Task<bool> Run(CancellationToken cancellationToken)
        {
            try
            {
                BinanceClient client = new BinanceClient();

                if(await client.CheckConnection())
                {
                    List<Coin> coins = StoredProcedures.GetActiveCoins();

                    foreach (Coin coin in coins)
                    {
                        try
                        {
                            if (!coin.BinanceListingDate.HasValue) continue;

                            List<Kline.APITickerKline> klines = new List<Kline.APITickerKline>();
                            Kline.TickerKline kline = await StoredProcedures.GetEarliestRecordedKline(coin, Kline.KlineInterval.OneHour);

                            if (kline == null || string.IsNullOrEmpty(kline?.KlineOpenTime))
                            {
                                klines.AddRange(await client.GetKlines(coin, Kline.KlineInterval.OneHour, 1000));
                            }
                            else
                            {
                                if (long.TryParse(kline.KlineOpenTime, out long earliestStored))
                                {
                                    DateTime earliestReadingDate = DateTimeOffset.FromUnixTimeMilliseconds(earliestStored).UtcDateTime;
                                    DateTime listingDate = DateTimeOffset.FromUnixTimeMilliseconds(coin.BinanceListingDate.Value).UtcDateTime;
                                    DateTime sevenYearsAgo = DateTime.UtcNow.AddYears(-7).AddDays(-1);
                                    DateTime stopBoundary = (listingDate > sevenYearsAgo) ? listingDate : sevenYearsAgo;

                                    if (earliestReadingDate > stopBoundary)
                                    {
                                        List<Kline.APITickerKline> klinesAPI = await client.GetKlines(coin, Kline.KlineInterval.OneHour, 1000, earliestReadingDate, null);

                                        if (klinesAPI != null && klinesAPI.Count > 0)
                                        {
                                            klines.AddRange(klinesAPI);
                                        }
                                    }
                                }
                            }

                            if (klines.Any())
                            {
 
                                await StoredProcedures.Insertklines(klines);
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unable to fetch klines for {coin.Ticker}: {ex.ToString()}");
                        }
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
