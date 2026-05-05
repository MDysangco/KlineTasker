using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrenchLooter.Models;

namespace TrenchLooter.CronTasks
{
    public class UpdateCoins
    {
        public static async Task<bool> Run(CancellationToken cancellationToken)
        {
            try
            {
                List<Coin> coins = StoredProcedures.GetActiveCoins();
                List<Coin> noDateCoinsList = coins.Where(coin => !coin.BinanceListingDate.HasValue).ToList();
  
                if(noDateCoinsList.Any())
                {
                    BinanceClient client = new BinanceClient();

                    if (await client.CheckConnection())
                    {
                        foreach (Coin coin in noDateCoinsList) {

                            DateTime startingEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                            List<Kline.APITickerKline> klines = await client.GetKlines(coin, Kline.KlineInterval.OneHour, 1, null, startingEpoch);

                            if (klines.Any())
                            {
                                Kline.APITickerKline kline = klines.First();
                                bool updated = await StoredProcedures.UpdateCoins(coin, true, kline.klineOpenTime.Value);

                                if (!updated)
                                {
                                    return false;
                                }
                            }
                        }
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
