using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CryptoWatchParse.ConsoleApp
{
    public class TargetModel
    {
        [JsonProperty("CreatedAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("Exchange")]
        public string Exchange { get; set; }

        [JsonProperty("AssetPair")]
        public string AssetPair { get; set; }

        [JsonProperty("Bid")]
        public double Bid { get; set; }

        [JsonProperty("Ask")]
        public double Ask { get; set; }

        [JsonProperty("Mid")]
        public double Mid { get; set; }

        [JsonProperty("VolEur")]
        public long VolEur { get; set; }

        [JsonProperty("Source")]
        public string Source { get; set; } = "cryptowat.ch OHLC";
    }

    public static class TargetModelFactory
    {
        /// <summary>
        /// [CloseTime,
        /// OpenPrice,
        /// HighPrice,
        /// LowPrice,
        /// ClosePrice,
        /// Volume,
        /// QuoteVolume]
        /// </summary>
        public static IEnumerable<TargetModel> Create(OhlcCandlestickModel model, string tmpOutputFile)
        {
            var tmp = model.Result.Resolutions.Select(x => new
            {
                CloseTime = x[0],
                CloseTimeIso = DateTimeOffset.FromUnixTimeSeconds((long)x[0]),
                OpenPrice = x[1],
                ClosePrice = x[4]
            });

            if (tmpOutputFile != null)
            {
                var tmpJson = JsonConvert.SerializeObject(tmp);
                File.WriteAllText(tmpOutputFile, tmpJson);
            }

            return null;
        }
    }
}
