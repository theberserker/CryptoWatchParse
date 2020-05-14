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
            var intermediateModels = model.Result.Resolutions.Select(x => new IntermediateModel
            {
                CloseTime = x[0],
                CloseTimeIso = DateTimeOffset.FromUnixTimeSeconds((long)x[0]).UtcDateTime,
                OpenPrice = x[1],
                ClosePrice = x[4]
            }).ToArray();

            if (tmpOutputFile != null)
            {
                var tmpJson = JsonConvert.SerializeObject(intermediateModels);
                File.WriteAllText(tmpOutputFile, tmpJson);
            }

            var targetVolumes = new[] {1M, 10, 100, 1000, 10_000};
            //var priceOption = new Expression<Func<IntermediateModel, decimal>>[] {x => x.OpenPrice, x => x.ClosePrice};

            // Za vsak record zgeneriras 5 target recordov z drugacnimi VolEur, ampak z istimi stevilkami.
            // V tvojem primeru, Bid = Mid = Ask. Open price ima timestamp polne ure (npr. 15:00:00),
            // close price pa ima timestamp ene sekunde pred polno uro (npr. 15:59:59)
            // This means - 5 records for each target volume and each of [OpenPrice, ClosedPrice] => 10 records per OhlcCandlestickModel.Result.Resolution
            var perVolumeResults = intermediateModels
                .SelectMany(im =>
                {
                    var baseTimeOffset = TimeSpan.FromHours(15);
                    var openTime = im.CloseTimeIso.Add(baseTimeOffset);
                    var closeTime = im.CloseTimeIso.Add(baseTimeOffset.Add(new TimeSpan(0, 59, 59)));

                    return targetVolumes
                        .Select(vol => im.ToOpenBitstampModel(vol, openTime))
                        .Union(targetVolumes.Select(vol => im.ToClosedBitstampModel(vol, closeTime)));
                });

            return perVolumeResults;
        }
    }

    public class IntermediateModel
    {
        public decimal CloseTime { get; set; }
        public DateTime CloseTimeIso { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal ClosePrice { get; set; }

        public TargetModel ToOpenBitstampModel(decimal volume, DateTime priceDateTime)
        {
            return new TargetModel(priceDateTime, "bitstamp", "bcheur", OpenPrice, OpenPrice, OpenPrice, volume);
        }

        public TargetModel ToClosedBitstampModel(decimal volume, DateTime priceDateTime)
        {
            return new TargetModel(priceDateTime, "bitstamp", "bcheur", ClosePrice, ClosePrice, ClosePrice, volume);
        }
    }
}
