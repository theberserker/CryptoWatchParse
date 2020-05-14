using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CryptoWatchParse.ConsoleApp
{
    class Program
    {
        private static readonly string BaseOutputFolder = @"C:\tmp";

        private static string GetOhlcRawOutputPath(long from, long to) 
            => Path.Combine(BaseOutputFolder, $"cryptowatch_ohlc_{from}_{to}.json");

        private static string GetTmpOutputFile()
            => Path.Combine(BaseOutputFolder, "tmp_model.json");

        private static string GetTargetOutputFileBitstamp()
            => Path.Combine(BaseOutputFolder, "target_bitstamp.json");

        private static string GetTargetOutputFileKraken()
            => Path.Combine(BaseOutputFolder, "target_kraken.json");

        private static string GetTargetOutputFileMeta()
            => Path.Combine(BaseOutputFolder, "target_meta.json");

        private static readonly JsonSerializerSettings jsonSettings
            = new JsonSerializerSettings { DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff'Z'" };

        static void Main(string[] args)
        {
            string targetFileBitstamp = GetTargetOutputFileBitstamp();
            string targetFileKraken = GetTargetOutputFileKraken();
            string targetFileMeta = GetTargetOutputFileMeta();
            //DeleteOutputFilesIfExistFiles(targetFileBitstamp, targetFileMeta);

            var from = new DateTimeOffset(DateTime.SpecifyKind(new DateTime(2018, 1, 1), DateTimeKind.Utc));
            var to = new DateTimeOffset(DateTime.UtcNow);

            var ohlcCandlestickModel = GetDailyCandle(from.ToUnixTimeSeconds(), to.ToUnixTimeSeconds()).GetAwaiter().GetResult();
            var bitstampResult = TargetModelFactory.Create(ohlcCandlestickModel, "bitstamp", 0, GetTmpOutputFile());
            var krakenResult = TargetModelFactory.Create(ohlcCandlestickModel, "kraken", 0, GetTmpOutputFile());
            var metaResult = TargetModelFactory.Create(ohlcCandlestickModel, "meta", 0.0075M, null);

            File.WriteAllLines(targetFileBitstamp, bitstampResult.Select(row => JsonConvert.SerializeObject(row, jsonSettings)));
            File.WriteAllLines(targetFileKraken, krakenResult.Select(row => JsonConvert.SerializeObject(row, jsonSettings)));
            File.WriteAllLines(targetFileMeta, metaResult.Select(row => JsonConvert.SerializeObject(row, jsonSettings)));
        }

        public static async Task<OhlcCandlestickModel> GetDailyCandle(long from, long to)
        {
            using (var httpClient = CreateHttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"markets/bitstamp/bcheur/ohlc?after={from}&before={to}&periods=86400");
                var result = await httpClient.SendAsync(request);
                if (!result.IsSuccessStatusCode)
                {
                    throw new Exception($"Error loading data. HTTP code:{result.StatusCode}");
                }

                string json = await result.Content.ReadAsStringAsync();
                await File.WriteAllTextAsync(GetOhlcRawOutputPath(from, to), json);
                return JsonConvert.DeserializeObject<OhlcCandlestickModel>(json);
            }
        }
        
        public static HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("https://api.cryptowat.ch/") };
            return httpClient;
        }

        private static void DeleteOutputFilesIfExistFiles(params string[] targetFiles)
        {
            foreach (var targetFile in targetFiles)
            {
                if (File.Exists(targetFile))
                {
                    File.Delete(targetFile);
                }
            }
        }
    }
}
