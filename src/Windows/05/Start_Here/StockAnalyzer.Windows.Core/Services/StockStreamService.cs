using System;
using System.Collections.Generic;
using System.Text;
using StockAnalyzer.Core.Domain;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace StockAnalyzer.Windows.Core.Services
{
    public interface IStockStreamService
    {
        IAsyncEnumerable<StockPrice> GetAllStockPrices(CancellationToken cancellationToken = default);
    }

    public class MockStockStreamService : IStockStreamService
    {
        public async IAsyncEnumerable<StockPrice> GetAllStockPrices(CancellationToken cancellationToken)
        {
            await Task.Delay(500);

            yield return new StockPrice { Ticker = "MSFT", Change = 0.5m, ChangePercent = 50 };

            await Task.Delay(500);

            yield return new StockPrice { Ticker = "MSFT", Change = 0.2m, ChangePercent = 20 };

            await Task.Delay(500);

            yield return new StockPrice { Ticker = "GOOGL", Change = 0.3m, ChangePercent = 30 };

            await Task.Delay(500);

            yield return new StockPrice { Ticker = "GOOGL", Change = 0.5m, ChangePercent = 50 };
        }
    }

    public class StockDiskStreamService : IStockStreamService
    {
        public async IAsyncEnumerable<StockPrice> GetAllStockPrices(CancellationToken cancellationToken)
        {
            using var stream = new StreamReader(File.OpenRead(@"StockPrices_small.csv"));

            await stream.ReadLineAsync();

            string line;

            while ((line = await stream.ReadLineAsync()) != null) 
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var segments = line.Split(',');
                for (var i = 0; i < segments.Length; i++) segments[i] = segments[i].Trim('\'', '"');

                var price = new StockPrice
                {
                    Ticker = segments[0],
                    TradeDate = DateTime.ParseExact(segments[1], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture),
                    Volume = Convert.ToInt32(segments[6], CultureInfo.InvariantCulture),
                    Change = Convert.ToDecimal(segments[7], CultureInfo.InvariantCulture),
                    ChangePercent = Convert.ToDecimal(segments[8], CultureInfo.InvariantCulture)
                };

                yield return price;
            }

        }
    }
}
