using System;
using System.IO;
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

        
        static void Main(string[] args)
        {
            var ohlcCandlestickModel = GetDailyCandle().GetAwaiter().GetResult();
            TargetModelFactory.Create(ohlcCandlestickModel, GetTmpOutputFile());
        }

        public static async Task<OhlcCandlestickModel> GetDailyCandle(long from = 1546300800, long to = 1577836800)
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
    }
}
