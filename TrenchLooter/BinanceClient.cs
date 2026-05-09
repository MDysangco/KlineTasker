using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using TrenchLooter.Models;
using Utils;
using Zyprix.Models;
using Kline = Zyprix.Models.Kline;

namespace TrenchLooter
{
	public class BinanceClient
	{
		private readonly string apiURL;
		private readonly string apiKey;
		private readonly string secretKey;

		public BinanceClient()
		{
			var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

			apiURL = config["BinanceURL"] ?? "";
			apiKey = config["BinanceAPIKey"] ?? "";
			secretKey = config["BinanceSecretKey"] ?? "";
		}

        public async Task<bool> CheckConnection()
		{
            try
            {
                string requestURL = $"{apiURL}/ping";
                JsonElement? response = await HttpHelper.MakeRequest<JsonElement>(HttpMethod.Get, requestURL, string.Empty);
                return response != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
		}

		public async Task<ServerTime?> CheckServerTime()
		{
            try
            {
                string requestURL = $"{apiURL}/time";
                return await HttpHelper.MakeRequest<ServerTime>(HttpMethod.Get, requestURL, string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new ServerTime();
            }
		}

		public async Task<TickerPrice?>GetSimplePrice(string ticker)
		{
            try
            {
                string requestURL = $"{apiURL}/ticker/price?symbol={ticker.ToUpper()}";
                return await HttpHelper.MakeRequest<TickerPrice>(HttpMethod.Get, requestURL, string.Empty);
            } 
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new TickerPrice();
            }
		}
        

        public async Task<List<Kline>> GetKlines(Coin coin, KlineInterval interval, int limit = 1, DateTime? endTime = null, DateTime? startTime = null)
        {
            try
            {
                string klineString = Utils.EnumExtension.GetDescription(interval);
                string requestURL = $"{apiURL}/klines?symbol={coin?.Ticker?.ToUpper()}&interval={klineString}&limit={limit}";

                if (endTime.HasValue)
                {
                    long endTimeMs = new DateTimeOffset(endTime.Value).ToUnixTimeMilliseconds();
                    requestURL += $"&endTime={endTimeMs}";
                }

                if (startTime.HasValue)
                {
                    long startTimeMs = new DateTimeOffset(startTime.Value).ToUnixTimeMilliseconds();
                    requestURL += $"&startTime={startTimeMs}";
                }

                string jsonString = await HttpHelper.MakeRequest(HttpMethod.Get, requestURL, string.Empty);

                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    return new List<Kline>();
                }

                JsonArray jsonArray = JsonSerializer.Deserialize<JsonArray>(jsonString);
                if (jsonArray == null || !jsonArray.Any())
                {
                    return new List<Kline>();
                }

                List<Kline> klines = new List<Kline>();

                foreach (JsonNode? node in jsonArray)
                {
                    klines.Add(new Kline
                    {
                        CoinId = coin.Id,
                        Interval = interval,
                        KlineOpenTime = node[0]?.ToString(),
                        OpenPrice = decimal.Parse(node[1]!.ToString()),
                        HighPrice = decimal.Parse(node[2]!.ToString()),
                        LowPrice = decimal.Parse(node[3]!.ToString()),
                        ClosePrice = decimal.Parse(node[4]!.ToString()),
                        Volume = decimal.Parse(node[5]!.ToString()),
                        NumberOfTrades = int.Parse(node[8]!.ToString()),
                        CreateDate = DateTime.UtcNow
                    });
                }

                return klines;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Kline>();
            }
        }

    }


}
