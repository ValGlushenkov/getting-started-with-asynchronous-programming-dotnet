using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.Windows;
using StockAnalyzer.Windows.Services;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace StockAnalyzer.Tests
{
    [TestClass]
    public class MockStockServiceTest
    {
        [TestMethod]
        public async Task Can_Load_All_MSFT_Stocks()
        {
            var service = new MockStockService();
            var stocks = await service.GetStockPricesFor("MSFT", CancellationToken.None);

            Assert.AreEqual(stocks.Count(), 2);
            Assert.AreEqual(stocks.Sum(stock => stock.Change), 0.7m);
        }
    }
}
