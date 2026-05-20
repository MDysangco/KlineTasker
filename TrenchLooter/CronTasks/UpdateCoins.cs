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

                coins = coins.Where(coin => !coin.BinanceListingDate.HasValue).ToList();
                if (coins == null || !coins.Any())
                {
                    return true;
                }

                if(!await binanceClient.CheckConnection())
                {
                    Console.WriteLine("Unable to connect to Binance.");
                    return false;
                }

				DateTime startingEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

				foreach (Coin coin in coins)
				{
					try
					{
						List<Kline> klines = await binanceClient.GetKlines(coin, KlineInterval.OneHour, 1, null, startingEpoch);

						if (klines.Any())
						{
							Kline kline = klines.First();

							if (!long.TryParse(kline.KlineOpenTime, out long klineOpenTime))
							{
								continue;
							}

							coin.BinanceListingDate = klineOpenTime;
							coin.Active = true;

							await zypryxClient.UpdateCoin(coin);
						}
					}
					catch (Exception ex)
                    {
						Console.WriteLine(ex.Message);
                        continue;
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
