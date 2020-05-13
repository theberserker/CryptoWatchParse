using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace CryptoWatchParse.ConsoleApp
{
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
            var tmp = model.Result.Resolutions.Select(x => new IntermediateModel
            {
                CloseTime = x[0],
                CloseTimeIso = DateTimeOffset.FromUnixTimeSeconds((long)x[0]),
                OpenPrice = x[1],
                ClosePrice = x[4]
            }).ToArray();

            if (tmpOutputFile != null)
            {
                var tmpJson = JsonConvert.SerializeObject(tmp);
                File.WriteAllText(tmpOutputFile, tmpJson);
            }

            var targetVolumes = new[] {1M, 10, 100, 1000, 10_000};
            var perVolumeResults = targetVolumes.SelectMany(vol => tmp.Select(t => t.ToOpenBitstampModel(vol)));
        }
    }

    public class IntermediateModel
    {
        public decimal CloseTime { get; set; }
        public DateTimeOffset CloseTimeIso { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal ClosePrice { get; set; }

        public TargetModel ToOpenBitstampModel(decimal volume)
        {
            return new TargetModel(CloseTimeIso, "bitstamp", "bcheur", OpenPrice, OpenPrice, OpenPrice, volume);
        }

        public TargetModel ToClosedBitstampModel(decimal volume)
        {
            return new TargetModel(CloseTimeIso, "bitstamp", "bcheur", ClosePrice, ClosePrice, ClosePrice, volume);
        }
    }
}
