using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using TrenchLooter.Models;

namespace TrenchLooter
{
    public class StoredProcedures
    {
        private static readonly string? _connectionString;

        static StoredProcedures()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public static List<Coin> GetAllCoins()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand("GetAllCoins", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    List<Coin> coins = new List<Coin>();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            coins.Add(reader.MapTo<Coin>());
                        }
                    }

                    return coins;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Coin>();
            }
        }

        public static List<Coin> GetActiveCoins()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand("GetActiveCoins", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    List<Coin> coins = new List<Coin>();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            coins.Add(reader.MapTo<Coin>());
                        }
                    }

                    return coins;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Coin>();
            }
        }

        public static async Task<bool> Insertklines(List<Kline.APITickerKline> klines)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand("InsertKlines", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    DataTable dt = new DataTable();
                    dt.Columns.Add("@CoinId", typeof(int));
                    dt.Columns.Add("@Interval", typeof(int));
                    dt.Columns.Add("@KlineOpenTime", typeof(string));
                    dt.Columns.Add("@OpenPrice", typeof(decimal));
                    dt.Columns.Add("@HighPrice", typeof(decimal));
                    dt.Columns.Add("@LowPrice", typeof(decimal));
                    dt.Columns.Add("@ClosePrice", typeof(decimal));
                    dt.Columns.Add("@Volume", typeof(decimal));
                    dt.Columns.Add("@NumberOfTrades", typeof(int));

                    foreach(Kline.APITickerKline kline in klines) {
                        dt.Rows.Add(
                            kline.coinId,
                            kline.interval,
                            kline.klineOpenTime,
                            kline.openPrice,
                            kline.highPrice,
                            kline.lowPrice,
                            kline.closePrice,
                            kline.volume,
                            kline.numberOfTrades
                        );
                    }

                    SqlParameter param = new SqlParameter("@Klines", SqlDbType.Structured)
                    {
                        TypeName = "dbo.KlineType",
                        Value = dt
                    };

                    cmd.Parameters.Add(param);
                    
                    return cmd.ExecuteNonQuery() > 0;
                }
            } 
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        public static async Task<int> DeleteKlinesByDateRange(long startDate, long endDate)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand("DeleteKlinesByDateRange", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("StartDate", SqlDbType.Decimal).Value = startDate;
                    cmd.Parameters.Add("EndDate", SqlDbType.Decimal).Value = endDate;
                    conn.Open();

                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }


        public static async Task<Kline.TickerKline> GetLatestRecordedKline(Coin coin, Kline.KlineInterval interval)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand("GetLatestRecordedKline", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("CoinId", SqlDbType.Int).Value = coin.Id;
                    cmd.Parameters.Add("Interval", SqlDbType.Int).Value = (int)interval;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return reader.MapTo<Kline.TickerKline>();
                        }
                    }
                }

                return new Kline.TickerKline();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new Kline.TickerKline();
            }
        }


        public static async Task<Kline.TickerKline> GetEarliestRecordedKline(Coin coin, Kline.KlineInterval interval)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand("GetEarliestRecordedKline", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("CoinId", SqlDbType.Int).Value = coin.Id;
                    cmd.Parameters.Add("Interval", SqlDbType.Int).Value = (int)interval;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return reader.MapTo<Kline.TickerKline>();
                        }
                    }
                }

                return new Kline.TickerKline();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new Kline.TickerKline();
            }
        }


        public static async Task<bool> UpdateCoins(Coin coin, bool active, long binanceListingDate)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand("UpdateCoin", conn))
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("CoinId", SqlDbType.Int).Value = coin.Id;
                    cmd.Parameters.Add("Active", SqlDbType.Bit).Value = active;
                    cmd.Parameters.Add("BinanceListingDate", SqlDbType.Decimal).Value = binanceListingDate;
                    conn.Open();

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }


        }
    }




}
