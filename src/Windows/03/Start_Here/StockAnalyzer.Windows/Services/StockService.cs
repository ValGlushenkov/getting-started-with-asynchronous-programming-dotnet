using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StockAnalyzer.Core.Domain;
using System.Threading;

namespace StockAnalyzer.Windows.Services
{
    public interface IStockService
    {
        Task<IEnumerable<StockPrice>> GetStockPricesFor(string ticker, CancellationToken cancelationToken);
    }
    public class StockService : IStockService
    {
        public async Task<IEnumerable<StockPrice>> GetStockPricesFor(string ticker,
            CancellationToken cancelationToken)
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync($"http://localhost:61363/api/stocks/{ticker}",
                    cancelationToken);

                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<IEnumerable<StockPrice>>(content);
            }
        }
    }

    public class MockStockService : IStockService
    {
        public async Task<IEnumerable<StockPrice>> GetStockPricesFor(string ticker,
            CancellationToken cancelationToken)
        {
            var stocks = new List<StockPrice> { 
                new StockPrice{ Ticker = "MSFT", Change = 0.5m, ChangePercent = 0.75m},
                new StockPrice{ Ticker = "MSFT", Change = 0.2m, ChangePercent = 0.15m},
                new StockPrice{ Ticker = "GOOGL", Change = 0.3m, ChangePercent = 0.25m},
                new StockPrice{ Ticker = "GOOGL", Change = 0.5m, ChangePercent = 0.65m}
            };
            //creates a task with a specific result.
            return await Task.FromResult(stocks.Where(stock => stock.Ticker == ticker));
        }
    }
}
