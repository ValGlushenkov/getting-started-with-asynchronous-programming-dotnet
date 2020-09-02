using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using StockAnalyzer.Core;
using System.Collections.Generic;
using StockAnalyzer.Windows;
using StockAnalyzer.Core.Domain;

namespace StockAnalyzer.Web.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            ViewBag.Title = "Home Page";

            // Let's make sure that we can load the files when you start the project!
            var store = new DataStore(HostingEnvironment.MapPath("~/bin"));

            await store.LoadStocks();

            return View();
        }

        [Route("Stock/{ticker}")]
        public async Task<ActionResult> Stock(string ticker)
        {
            var context = HttpContext.ApplicationInstance.Context;

            var data = await GetStocks();

            return View(data[ticker]);
        }

        public async Task<Dictionary<string, IEnumerable<StockPrice>>> GetStocks()
        {
            var store = new DataStore("");

            var data = await store
                .LoadStocks()
                .ConfigureAwait(false);

            var context = HttpContext.ApplicationInstance.Context;

            return data;
        }
    }
}
