using Newtonsoft.Json;
using System;

namespace CryptoWatchParse.ConsoleApp
{
    public class TargetModel
    {
        public TargetModel(DateTimeOffset createdAt, string exchange, string assetPair, decimal bid, decimal ask, decimal mid, decimal volEur)
        {
            CreatedAt = createdAt;
            Exchange = exchange;
            AssetPair = assetPair;
            Bid = bid;
            Ask = ask;
            Mid = mid;
            VolEur = volEur;
        }

        [JsonProperty("CreatedAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("Exchange")]
        public string Exchange { get; set; }

        [JsonProperty("AssetPair")]
        public string AssetPair { get; set; }

        [JsonProperty("Bid")]
        public decimal Bid { get; set; }

        [JsonProperty("Ask")]
        public decimal Ask { get; set; }

        [JsonProperty("Mid")]
        public decimal Mid { get; set; }

        [JsonProperty("VolEur")]
        public decimal VolEur { get; set; }

        [JsonProperty("Source")]
        public string Source { get; set; } = "cryptowat.ch OHLC";
    }
}
