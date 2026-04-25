using System.ComponentModel;
using System.Text.Json.Nodes;

namespace TrenchLooter.Models
{
    public class Kline
    {

        public class TickerKline 
        { 
            public int? Id { get; set; }
            public int? CoinId { get; set; }
            public KlineInterval? Interval { get; set; }
            public string? KlineOpenTime { get; set; }
            public decimal? OpenPrice { get; set; }
            public decimal? HighPrice { get; set; }
            public decimal? LowPrice { get; set; }
            public decimal? ClosePrice { get; set; }
            public decimal? Volume { get; set; }
            public int? NumberOfTrades { get; set; }
            public DateTime? CreateDate { get; set; }
        }


        public class APITickerKline
        {
            //Returned by Binance API
            public long? klineOpenTime { get; set; }
            public string? openPrice { get; set; }
            public string? highPrice { get; set; }
            public string? lowPrice { get; set; }
            public string? closePrice { get; set; }
            public string? volume { get; set; }
            public long? klineCloseTime { get; set; }
            public string? quoteAssetVolume { get; set; }
            public int? numberOfTrades { get; set; }
            public string? takerBuyBaseAssetVolume { get; set; }
            public string? takerBuyQuoteAssetVolume { get; set; }
            public string? unusedField { get; set; }

            //Set manually
            public int? coinId { get; set; }
            public KlineInterval? interval { get; set; }
        }

        public enum KlineInterval
        {
            [Description("1s")]
            OneSecond = 0,

            [Description("1m")]
            OneMinute = 1,

            [Description("3m")]
            ThreeMinutes = 2,

            [Description("5m")]
            FiveMinutes = 3,

            [Description("15m")]
            FifteenMinutes = 4,

            [Description("30m")]
            ThirtyMinutes = 5,

            [Description("1h")]
            OneHour = 6,

            [Description("2h")]
            TwoHours = 7,

            [Description("4h")]
            FourHours = 8,

            [Description("6h")]
            SixHours = 9,

            [Description("8h")]
            EightHours = 10,

            [Description("12h")]
            TwelveHours = 11,

            [Description("1d")]
            OneDay = 12,

            [Description("3d")]
            ThreeDays = 13,

            [Description("1w")]
            OneWeek = 14,

            [Description("1mo")]
            OneMonth = 15
        }


        internal static List<APITickerKline> ParseJson(JsonArray json)
        {
            List<APITickerKline> klines = new List<APITickerKline>();
            foreach(JsonNode? node in json)
            {
                klines.Add(new APITickerKline() {

                    klineOpenTime = long.Parse(node[0].ToString()),
                    openPrice = node[1].ToString(),
                    highPrice = node[2].ToString(),
                    lowPrice = node[3].ToString(),
                    closePrice = node[4].ToString(),
                    volume = node[5].ToString(),
                    klineCloseTime = long.Parse(node[6].ToString()),
                    quoteAssetVolume = node[7].ToString(),
                    numberOfTrades = int.Parse(node[8].ToString()),
                    takerBuyBaseAssetVolume = node[9].ToString(),
                    takerBuyQuoteAssetVolume = node[10].ToString(),
                    unusedField = node[11].ToString(),

                });
            }

            return klines;
        }
    }



}


