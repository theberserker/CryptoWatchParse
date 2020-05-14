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
        public static IEnumerable<TargetModel> Create(OhlcCandlestickModel model, string exchange, decimal pricePadding, string tmpOutputFile)
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
            //var priceOption = new Func<IntermediateModel, decimal>[] {x => x.OpenPrice, x => x.ClosePrice};

            // Za vsak record zgeneriras 5 target recordov z drugacnimi VolEur, ampak z istimi stevilkami.
            // V tvojem primeru, Bid = Mid = Ask. Open price ima timestamp polne ure (npr. 15:00:00),
            // close price pa ima timestamp ene sekunde pred polno uro (npr. 15:59:59)
            //
            // This means - 5 records for each target volume and each of [OpenPrice, ClosedPrice] => 10 records per OhlcCandlestickModel.Result.Resolution
            var perVolumeResults = intermediateModels
                .SelectMany(im =>
                {
                    var baseTimeOffset = TimeSpan.FromHours(15);
                    var openTime = im.CloseTimeIso.Add(baseTimeOffset);
                    var closeTime = im.CloseTimeIso.Add(baseTimeOffset.Add(new TimeSpan(0, 59, 59)));

                    return targetVolumes
                        .Select(vol => im.ToOpenBitstampModel(vol, exchange, pricePadding, openTime))
                        .Union(targetVolumes.Select(vol => im.ToClosedBitstampModel(vol, exchange, pricePadding, closeTime)));
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

        public TargetModel ToOpenBitstampModel(decimal volume, string exchange, decimal pricePadding, DateTime priceDateTime)
        {
            var padding = GetPadding(pricePadding);
            return pricePadding == decimal.Zero
                ? new TargetModel(priceDateTime, exchange, "bcheur", OpenPrice, OpenPrice, OpenPrice, volume)
                : new TargetModel(priceDateTime, exchange, "bcheur", OpenPrice * padding.bid, OpenPrice * padding.ask, OpenPrice, volume);
        }

        public TargetModel ToClosedBitstampModel(decimal volume, string exchange, decimal pricePadding, DateTime priceDateTime)
        {
            var padding = GetPadding(pricePadding);
            return pricePadding == decimal.Zero
                ? new TargetModel(priceDateTime, exchange, "bcheur", ClosePrice, ClosePrice, ClosePrice, volume)
                : new TargetModel(priceDateTime, exchange, "bcheur", ClosePrice * padding.bid, ClosePrice * padding.ask, ClosePrice, volume);
        }

        private (decimal bid, decimal ask) GetPadding(decimal pricePadding)
            => ((1 - pricePadding), ask:(1 + pricePadding));
    }
}
