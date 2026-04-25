using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using TrenchLooter.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static TrenchLooter.Models.Kline;

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

        private static async Task<string?> MakeRequest(HttpMethod method, string url, bool secure)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage? request = new HttpRequestMessage(method, url);
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                } 
                else
                {
                    Console.WriteLine($"Unable to make request: {response.StatusCode}");
                }
                return default;
            }
        }

        private static async Task<T?> MakeRequest<T>(HttpMethod method, string url, bool secure)
        {
			string? json = await MakeRequest(method, url, secure);
			
			if(!string.IsNullOrEmpty(json))
			{
                return JsonSerializer.Deserialize<T>(json);
            }

            return default;
        }


        public async Task<bool> CheckConnection()
		{
            try
            {
                string requestURL = $"{apiURL}/ping";
                JsonElement? response = await MakeRequest<JsonElement>(HttpMethod.Get, requestURL, false);
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
                return await MakeRequest<ServerTime>(HttpMethod.Get, requestURL, false);
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
                return await MakeRequest<TickerPrice>(HttpMethod.Get, requestURL, false);
            } 
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new TickerPrice();
            }
		}


        public async Task<List<APITickerKline>> GetKlines(Coin coin, KlineInterval interval, int limit = 1, DateTime? endTime = null, DateTime? startTime = null)
        {
            try
            {
                string klineString = EnumExtension.GetDescription(interval);
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

                string jsonString = await MakeRequest(HttpMethod.Get, requestURL, false);

                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    return new List<APITickerKline>();
                }

                JsonArray jsonArray = JsonSerializer.Deserialize<JsonArray>(jsonString);
                if (jsonArray == null || !jsonArray.Any())
                {
                    return new List<APITickerKline>();
                }

                List<APITickerKline> klines = ParseJson(jsonArray);
                klines.ForEach(kline => {
                    kline.coinId = coin?.Id;
                    kline.interval = interval;
                });

                return klines;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<APITickerKline>();
            }
        }

    }


}
