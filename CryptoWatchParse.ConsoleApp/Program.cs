using System;
using System.Collections.Generic;
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
        private static readonly string BaseOutputFolder = @"C:\Users\Andrej Spilak\Downloads\bchdata";

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
            //MainDataTransformOnly();
            //MainCryptoWatch();
        }

        public static void MainDataTransformOnly()
        {
            var outDir = Path.Combine(BaseOutputFolder, "out");
            var files = Directory.GetFiles(BaseOutputFolder);
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                Console.WriteLine($"{DateTime.Now.TimeOfDay}: Starting writing new file {fileName}.");
                var outFilePath = Path.Combine(outDir, fileName);
                DeleteFilesIfExist(outFilePath);

                var lines = File.ReadAllLines(file);
                var mutatedLines = new List<string>();
                foreach (var line in lines)
                {
                    var mutableModel = JsonConvert.DeserializeObject<TargetModel>(line);
                    var spreadMultiplier = TargetModelFactory.GetPriceFactor(0.0075M);
                    mutableModel.Exchange = "meta";
                    mutableModel.Ask = mutableModel.Ask * spreadMultiplier.ask;
                    mutableModel.Bid = mutableModel.Bid * spreadMultiplier.bid;
                    mutableModel.Source = "Bitstamp +0.75%";

                    mutatedLines.Add(JsonConvert.SerializeObject(mutableModel, jsonSettings));
                }
                File.WriteAllLines(outFilePath, mutatedLines);
                Console.WriteLine($"{DateTime.Now.TimeOfDay}:Done writing new file {fileName}");
            }
        }

        public static void MainCryptoWatch()
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
                //string requestUri = $"markets/bitstamp/bcheur/ohlc?after={from}&before={to}&periods=86400";
                string requestUri = "markets/bitstamp/bcheur/ohlc?periods=86400";
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
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

        private static void DeleteFilesIfExist(params string[] targetFiles)
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
