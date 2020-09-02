using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Newtonsoft.Json;
using StockAnalyzer.Core.Domain;
using StockAnalyzer.Windows.Services;

namespace StockAnalyzer.Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        CancellationTokenSource cancellationTokenSource = null;

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            #region Before loading stock data
            var watch = new Stopwatch();
            watch.Start();
            StockProgress.Visibility = Visibility.Visible;
            StockProgress.IsIndeterminate = true;

            Search.Content = "Cancel";
            #endregion

            if(cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();

            //register a delegate that is called when cancelation token is canceled
            cancellationTokenSource.Token.Register(() => {
                Notes.Text = "Cancelation requested";
            });

            //Executes on a different thread from UI one.
            var loadedLinesTask = SearchForStocks(cancellationTokenSource.Token);

            try
            {
                var tickers = Ticker.Text.Split(',', ' ');

                var service = new StockService();
                //Concurrent bag is a thread safe collection that allows us to add data from different threads.
                var stocks = new ConcurrentBag<StockPrice>();

                var tickerLoadingTasks = new List<Task<IEnumerable<StockPrice>>>();
                foreach(var ticker in tickers)
                {
                    //loads a new thread for each ticker
                    var loadTask = service.GetStockPricesFor(Ticker.Text, cancellationTokenSource.Token)
                        .ContinueWith(t => { 
                            //Adds 5 stocks from each ticker as they come in from the service
                            foreach(var stock in t.Result.Take(5))
                            {
                                stocks.Add(stock);
                            }

                            Dispatcher.Invoke(() => {
                                Stocks.ItemsSource = stocks.ToArray();
                            });

                            return t.Result;
                        });

                    tickerLoadingTasks.Add(loadTask);
                }

                //Completes all threads
                var allStocksLoadingTask = Task.WhenAll(tickerLoadingTasks);
                #region Handling Timeout
                //var timeoutTask = Task.Delay(2000); // 2 seconds
                //var completedTasks = await Task.WhenAny(timeoutTask, allStocksLoadingTask);

                //if(completedTasks == timeoutTask)
                //{
                //    cancellationTokenSource.Cancel();
                //    cancellationTokenSource = null;
                //    throw new Exception("Timeout!");
                //}
                #endregion

                await allStocksLoadingTask;
            }
            catch(Exception ex)
            {
                Notes.Text += ex.Message + Environment.NewLine;
            }            
            
            #region After stock data is loaded
            StocksStatus.Text = $"Loaded stocks for {Ticker.Text} in {watch.ElapsedMilliseconds}ms";
            StockProgress.Visibility = Visibility.Hidden;
            Search.Content = "Search";
            #endregion


            cancellationTokenSource = null;
        }

        private Task<List<string>> SearchForStocks(CancellationToken cancellationToken)
        {
            var loadLinesTask = Task.Run(async () =>
            {
                var lines = new List<string>();

                using (var stream = new StreamReader(File.OpenRead(@"StockPrices_small.csv")))
                {
                    string line;
                    while ((line = await stream.ReadLineAsync()) != null)
                    {
                        if(cancellationToken.IsCancellationRequested)
                        {
                            return lines;
                        }
                        lines.Add(line);
                    }
                }

                return lines;
            }, cancellationToken);

            return loadLinesTask;
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));

            e.Handled = true;
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
