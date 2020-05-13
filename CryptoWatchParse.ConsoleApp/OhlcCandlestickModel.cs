using Newtonsoft.Json;

namespace CryptoWatchParse.ConsoleApp
{
    public partial class OhlcCandlestickModel
    {
        [JsonProperty("result")]
        public Result Result { get; set; }

        [JsonProperty("allowance")]
        public Allowance Allowance { get; set; }
    }

    public partial class Allowance
    {
        [JsonProperty("cost")]
        public long Cost { get; set; }

        [JsonProperty("remaining")]
        public long Remaining { get; set; }

        [JsonProperty("remainingPaid")]
        public long RemainingPaid { get; set; }

        [JsonProperty("upgrade")]
        public string Upgrade { get; set; }
    }

    public partial class Result
    {
        /// <summary>
        /// Keeps the daily resolution results, by the following order:
        /// [CloseTime,
        /// OpenPrice,
        /// HighPrice,
        /// LowPrice,
        /// ClosePrice,
        /// Volume,
        /// QuoteVolume]
        /// </summary>
        [JsonProperty("86400")]
        public decimal[][] Resolutions { get; set; }
    }
}
