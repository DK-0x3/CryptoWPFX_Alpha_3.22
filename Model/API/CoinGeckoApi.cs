using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using SciChart.Charting.Model.DataSeries;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;

namespace CryptoWPFX.Model.API
{
    public class CoinGeckoApi

    {
        HttpClient _httpClient = new HttpClient();
        public CoinGeckoApi()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
        }
        public async Task<List<string>> GetSupportedCurrenciesAsync()
        {
            string url = $"https://api.coingecko.com/api/v3/simple/supported_vs_currencies";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                string jsonResponse = "";

                if (response.IsSuccessStatusCode)
                    jsonResponse = await response.Content.ReadAsStringAsync();
                else
                    jsonResponse = APIResponceImitation.GetSupportedCurrenciesAsync();

                List<string> supportedCurrencySymbols = JsonConvert.DeserializeObject<List<string>>(jsonResponse);

                return supportedCurrencySymbols;
            }
            catch (Exception)
            {

            }

            return new List<string>();
        }
        public async Task<List<CryptoCurrency>> GetTopNCurrenciesAsync(int topN, int pageNum)
        {
            string validatedTopN = topN > 500 ? "500" : (topN < 0 ? "1" : topN.ToString());
            string validatedpageNum = pageNum < 0 ? "1" : pageNum.ToString();

            string url = $"https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page={validatedTopN}&page={validatedpageNum}&sparkline=false&locale=en";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                string jsonResponse = "";

                if (response.IsSuccessStatusCode)
                    jsonResponse = await response.Content.ReadAsStringAsync();
                else
                    jsonResponse = APIResponceImitation.GetTopNCurrenciesAsync();

                var topNCurrencies = JsonConvert.DeserializeObject<List<CryptoCurrency>>(jsonResponse);

                return topNCurrencies;
            }
            catch (Exception)
            {
                // Обработка ошибок здесь
            }

            return new List<CryptoCurrency>();
        }
        public async Task<decimal> GetCurrencyPriceByIdAsync(string id, string targetCurrencyId)
        {
            string url = $"https://api.coingecko.com/api/v3/simple/price?ids={id}&vs_currencies={targetCurrencyId}&include_market_cap=false&include_24hr_vol=false&include_24hr_change=false&include_last_updated_at=false";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                string jsonResponse = "";

                if (response.IsSuccessStatusCode)
                    jsonResponse = await response.Content.ReadAsStringAsync();
                else
                    jsonResponse = APIResponceImitation.GetCurrencyPriceByIdAsync();

                CurrencyDetails currencyDetails = await GetCurrencyDetailsByIdAsync(id, targetCurrencyId);

                decimal price = currencyDetails.Price;

                return price;
            }
            catch (Exception)
            {

            }

            return -1;
        }
        public async Task<CurrencyDetails> GetCurrencyDetailsByIdAsync(string id, string targetCurrencyId)
        {
            string url = $"https://api.coingecko.com/api/v3/coins/{id}?localization=false&tickers=false&market_data=true&community_data=false&developer_data=false&sparkline=false";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                string jsonResponse = "";

                if (response.IsSuccessStatusCode)
                    jsonResponse = await response.Content.ReadAsStringAsync();
                else
                    jsonResponse = APIResponceImitation.GetCurrencyDetailsByIdAsync();

                JObject jsonObject = JObject.Parse(jsonResponse);

                CurrencyDetails CryptoCurrency = new CurrencyDetails();
                CryptoCurrency.Id = (string)jsonObject["id"];
                CryptoCurrency.Name = (string)jsonObject["symbol"];
                CryptoCurrency.Symbol = (string)jsonObject["symbol"];
                CryptoCurrency.Image = (string)jsonObject["image"]["large"];
                CryptoCurrency.Price = (decimal)jsonObject["market_data"]["current_price"][targetCurrencyId];
                CryptoCurrency.Volume = (decimal)jsonObject["market_data"]["total_volume"][targetCurrencyId];
                return CryptoCurrency;
            }
            catch (Exception)
            {

            }

            return new CurrencyDetails();
        }


        //xxx//
        public static async Task<List<TickerData>> GetActualBurse(string TokenID)
        {
            //var client = new RestClient("https://api.coingecko.com");
            //var request = new RestRequest($"/api/v3/coins/{TokenID}?localization=false&tickers=true&market_data=true&community_data=false&developer_data=false&sparkline=false");
            var options = new RestClientOptions($"https://api.coingecko.com/api/v3/coins/{TokenID}?localization=false&tickers=true&market_data=true&community_data=false&developer_data=false&sparkline=false");
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("x-cg-demo-api-key", Properties.Settings.Default.APIKeyCoinGecko);
            var response = await client.GetAsync(request);


                /// Разбор JSON-ответа
                JsonDocument document = JsonDocument.Parse(response.Content);
                // Получаем массив "tickers"
                JsonElement tickersElement = document.RootElement.GetProperty("tickers");

                // Создаем список для хранения данных тикеров
                List<TickerData> tickerDataList = new List<TickerData>();
                // Перебираем элементы массива "tickers"
                foreach (JsonElement tickerElement in tickersElement.EnumerateArray())
                {
                    TickerData tickerData = new TickerData();
                    // Извлекаем значения свойств тикера
                    tickerData.Base = tickerElement.GetProperty("base").GetString();
                    tickerData.Target = tickerElement.GetProperty("target").GetString();
                    tickerData.LastPrice = tickerElement.GetProperty("last").GetDouble();
                    tickerData.TradeURL = tickerElement.GetProperty("trade_url").GetString();

                    JsonElement marketElement = tickerElement.GetProperty("market");
                    // Извлекаем значение свойства "name" из объекта "market"
                    tickerData.Name = marketElement.GetProperty("name").GetString();

                    tickerDataList.Add(tickerData);
                }
                return tickerDataList;
            
        }

        public static async Task<JsonElement> GetInfoTokenToIDFull(string TokenID)
        {
            //var client = new RestClient("https://api.coingecko.com");
            //var request = new RestRequest($"/api/v3/coins/{TokenID}?localization=false&tickers=true&market_data=true&community_data=false&developer_data=false&sparkline=false");
            var options = new RestClientOptions($"https://api.coingecko.com/api/v3/coins/{TokenID}?localization=false&tickers=true&market_data=true&community_data=false&developer_data=false&sparkline=false");
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("x-cg-demo-api-key", Properties.Settings.Default.APIKeyCoinGecko);
            var response = await client.GetAsync(request);


            Console.WriteLine(response.Content);


            // Разбор JSON-ответа
            JsonDocument document = JsonDocument.Parse(response.Content);

            // Получение корневого элемента
            JsonElement root = document.RootElement;

            // Поиск элемента current_price
            JsonElement currentPriceElement = root.GetProperty("market_data").GetProperty("current_price");

            //double currentPriceUsd = currentPriceElement.GetProperty("rub").GetDouble();
            return currentPriceElement;

        }

        public async static Task<Dictionary<string, object>> GetInfoTokenToID(string TokenID, string cyrrency)
        {
            //var client = new RestClient("https://api.coingecko.com");
            //var request = new RestRequest($"/api/v3/coins/markets?vs_currency={cyrrency}&ids={TokenID}&order=market_cap_desc&per_page=100&page=1&sparkline=false&price_change_percentage=1h%2C24h%2C7d%2C14d%2C30d%2C1y&locale=ru&precision=8");
            var options = new RestClientOptions($"https://api.coingecko.com/api/v3/coins/markets?vs_currency={cyrrency}&ids={TokenID}&order=market_cap_desc&per_page=100&page=1&sparkline=false&price_change_percentage=1h%2C24h%2C7d%2C14d%2C30d%2C1y&locale=ru&precision=8");
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("x-cg-demo-api-key", Properties.Settings.Default.APIKeyCoinGecko);
            var response = await client.GetAsync(request);

            string content = response.Content.Trim();
            if (content.StartsWith("[") && content.EndsWith("]"))
            {
                content = content.Substring(1, content.Length - 2);
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            
                try
                {
                    dictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(content);

                }
                catch 
                {
                dictionary.Add("symbol", response.Content);
                }
            
            
            return dictionary;
            
        }

        public enum CoinField
        {
            Id,
            Symbol,
            Name,
            Image,
            Current_Price,
            Market_Cap,
            Market_Cap_Rank,
            Fully_Diluted_Valuation,
            Total_Volume,
            High_24h,
            Low_24h,
            Price_Change_24h,
            Price_Change_Percentage_24h,
            Market_Cap_Change_24h,
            Market_Cap_Change_Percentage_24h,
            Circulating_Supply,
            Total_Supply,
            Max_Supply,
            Ath,
            Ath_Change_Percentage,
            Ath_Date,
            Atl,
            Atl_Change_Percentage,
            Atl_Date,
            Roi,
            Last_Updated,
            Price_Change_Percentage_1h_In_Currency,
            Price_Change_Percentage_1y_In_Currency,
            Price_Change_Percentage_14d_In_Currency,
            Price_Change_Percentage_24h_In_Currency,
            Price_Change_Percentage_30d_In_Currency,
            Price_Change_Percentage_7d_In_Currency
        }

        public static async Task<XyDataSeries<DateTime, double>> GetActualChartToken(string TokenID, string currency, string Days)
        {
            //var client = new RestClient("https://api.coingecko.com");
            //var request = new RestRequest($"/api/v3/coins/{TokenID}/market_chart?vs_currency={currency}&days={Days}&precision=8");
            var options = new RestClientOptions($"https://api.coingecko.com/api/v3/coins/{TokenID}/market_chart?vs_currency={currency}&days={Days}&precision=8");
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("x-cg-demo-api-key", Properties.Settings.Default.APIKeyCoinGecko);
            var response = await client.GetAsync(request);

            var series = new XyDataSeries<DateTime, double>();

            if (response.IsSuccessful)
            {
                // Разбор JSON-ответа
                JsonDocument document = JsonDocument.Parse(response.Content);

                JsonElement root = document.RootElement;

                // Извлечение данных о цене и капитализации рынка
                JsonElement pricesArray = root.GetProperty("prices");
                JsonElement marketCapArray = root.GetProperty("market_caps");

                // Преобразование данных в массивы
                List<double> prices = pricesArray.EnumerateArray().Select(p => p[1].GetDouble()).ToList();
                List<double> dateUnix = pricesArray.EnumerateArray().Select(p => p[0].GetDouble()).ToList();

                for (int i = 0; i < prices.Count; i++)
                {
                    series.Append(DateTimeOffset.FromUnixTimeMilliseconds((long)dateUnix[i]).DateTime, prices[i]);
                }
                return series;
            }
            else { return null; }
        }

        // Класс для хранения данных тикера
        public class TickerData
        {
            public string Base { get; set; }
            public string Target { get; set; }
            public double LastPrice { get; set; }
            public string Name { get; set; }
            public string TradeURL { get; set; }
        }

        public static async Task<Coin[]> GetTokensInfoToIDs(List<string> TokensID)
        {
            string tokens = "";
            foreach (string token in TokensID)
            {
                tokens = tokens + "%2C%20" + token;
            }
            tokens = tokens.Substring(6);   
            var options = new RestClientOptions($"https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&ids={tokens}&order=market_cap_desc&per_page=250&page=1&sparkline=false&price_change_percentage=1h%2C24h%2C7d%2C14d%2C30d%2C90d%2C1y&locale=en&precision=8");
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("x-cg-demo-api-key", "CG-pibZCCfRXjV16buMmrrk16SU");
            var response = await client.GetAsync(request);

            Coin[] coins = JsonConvert.DeserializeObject<Coin[]>(response.Content);
            return coins;
        }

        public class Coin
        {
            public string? Id { get; set; }
            public string? Symbol { get; set; }
            public string? Name { get; set; }
            public string? Image { get; set; }
            public double? current_price { get; set; }
            public long? Market_Cap { get; set; }
            public int? Market_Cap_Rank { get; set; }
            public long? Fully_Diluted_Valuation { get; set; }
            public long? Total_Volume { get; set; }
            public double? High_24h { get; set; }
            public double? Low_24h { get; set; }
            public double? Price_Change_24h { get; set; }
            public double? Price_Change_Percentage_24h { get; set; }
            public long? Market_Cap_Change_24h { get; set; }
            public double? Market_Cap_Change_Percentage_24h { get; set; }
            public long? Circulating_Supply { get; set; }
            public long? Total_Supply { get; set; }
            public long? Max_Supply { get; set; }
            public double Ath { get; set; }
            public double? Ath_Change_Percentage { get; set; }
            public DateTime? Ath_Date { get; set; }
            public double? Atl { get; set; }
            public double? Atl_Change_Percentage { get; set; }
            public DateTime? Atl_Date { get; set; }
            public object? Roi { get; set; }
            public DateTime? Last_Updated { get; set; }
            public double? price_change_percentage_14d_in_currency { get; set; }
            public double? Price_Change_Percentage_1h_In_Currency { get; set; }
            public double? Price_Change_Percentage_1y_In_Currency { get; set; }
            public double? Price_Change_Percentage_24h_In_Currency { get; set; }
            public double? Price_Change_Percentage_30d_In_Currency { get; set; }
            public double? Price_Change_Percentage_7d_In_Currency { get; set; }

            public SolidColorBrush colorPrice { get; set; }

            public Coin()
            {
                colorPrice = Brushes.LimeGreen;
            }
        }
    }
}
