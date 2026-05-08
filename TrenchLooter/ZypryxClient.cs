using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using Zyprix.Models;

namespace TrenchLooter
{
    public class ZypryxClient
    {
        private readonly string apiURL;
        private readonly string apiKey;
        private readonly string secretKey;

        public ZypryxClient()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            apiURL = config["ZyprixAPIUrl"] ?? "";
            apiKey = config["ZyprixAPIKey"] ?? "";
            secretKey = config["ZyprixSecretKey"] ?? "";
        }

        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

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
        private static async Task<string?> MakePost(string url, string jsonBody, bool secure)
        {
            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                Console.WriteLine($"Unable to make POST request: {response.StatusCode}");
                return default;
            }
        }

        private static async Task<T?> MakeRequest<T>(HttpMethod method, string url, bool secure)
        {
            string? json = await MakeRequest(method, url, secure);

            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<T>(json, jsonOptions);
            }

            return default;
        }

        private static async Task<T?> MakePost<T>(string url, object body, bool secure)
        {
            string jsonBody = JsonSerializer.Serialize(body);

            string? json = await MakePost(url, jsonBody, secure);

            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<T>(json, jsonOptions);
            }

            return default;
        }


        #region Coin Endpoints

        public async Task<List<Coin>?> GetAllCoins()
        {
            try
            {
                string requestURL = $"{apiURL}/coin";
                return await MakeRequest<List<Coin>>(HttpMethod.Get, requestURL, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Coin>();
            }
        }

        public async Task<List<Coin>?> GetActiveCoins()
        {
            try
            {
                string requestURL = $"{apiURL}/coin/active";
                return await MakeRequest<List<Coin>>(HttpMethod.Get, requestURL, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Coin>();
            }
        }

        public async Task<Coin?> GetCoin(int coinId)
        {
            try
            {
                string requestURL = $"{apiURL}/coin/{coinId}";
                return await MakeRequest<Coin>(HttpMethod.Get, requestURL, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new Coin();
            }
        }

        public async Task<bool> UpdateCoin(Coin coin)
        {
            try
            {
                string requestURL = $"{apiURL}/coin/update";
                return await MakePost<bool>(requestURL, coin, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        #endregion

        #region Kline Endpoints

        public async Task<Kline?> GetLatestKline(int coinId, KlineInterval interval)
        {
            try
            {
                string requestURL = $"{apiURL}/kline/latest?coinId={coinId}&interval={(int)interval}";
                return await MakeRequest<Kline>(HttpMethod.Get, requestURL, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new Kline();
            }
        }

        public async Task<Kline?> GetEarliestKline(int coinId, KlineInterval interval)
        {
            try
            {
                string requestURL = $"{apiURL}/kline/earliest?coinId={coinId}&interval={(int)interval}";
                return await MakeRequest<Kline>(HttpMethod.Get, requestURL, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new Kline();
            }
        }

        public async Task<bool> InsertKlines(List<Kline> klines)
        {
            try
            {
                string requestURL = $"{apiURL}/kline/insert";
                return await MakePost<bool>(requestURL, klines, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<int> DeleteKlinesByDateRange(long startDate, long endDate)
        {
            try
            {
                string requestURL = $"{apiURL}/kline?startDate={startDate}&endDate={endDate}";
                return await MakeRequest<int>(HttpMethod.Delete, requestURL, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        #endregion
    }
}
