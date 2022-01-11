using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using stock_quote_alert.intefaces;

namespace stock_quote_alert.classes
{
    /// <summary>
    /// Allows to check the quotes prices using API calls. Implements the IObservable interface for the observer pattern.
    /// </summary>
    internal class StockObservable : IObservable
    {
        private string key;
        private readonly int sleepTime = 900000; // 900000 milisecond = 15 minutes

        private List<IObserver> obsList = new List<IObserver> (); // Allow multiple obervers for this class
        
        private List<Stock> stocksAlert = new List<Stock>(); // This list is useful to not alerting twice about the same stock

        /// <summary>
        /// Initializes an instance of StockObserver. Needs a valid key of the HG Brasil finance API.
        /// </summary>
        /// <param name="key"></param>
        public StockObservable(string key)
        {
            this.key = key;
        }


        /// <summary>
        /// Checks the price of the stock in the parameters every 15 minutes and alert the obervers when the stock price is over or under its referece prices.
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        public async Task CheckQuotes(Stock stock)
        {
            string QUERY_URL = string.Format("https://api.hgbrasil.com/finance/stock_price?key={0}&symbol={1}", this.key, stock.symbol);
            Uri queryUri = new Uri(QUERY_URL);

            HttpClient client = new HttpClient();
            while (true)
            {
                HttpResponseMessage response = await client.GetAsync(QUERY_URL);
                response.EnsureSuccessStatusCode();
                JObject json = JObject.Parse(await response.Content.ReadAsStringAsync());
                try
                {
                    if(!bool.Parse(json["valid_key"].ToString()))
                    {
                        Console.WriteLine("Error at getting prices. Check key.");
                        return;
                    }
                    
                    if (!((JObject)json["results"]).ContainsKey(stock.symbol) || ((JObject)json["results"][stock.symbol]).ContainsKey("error"))
                    {
                        Console.WriteLine("Error at getting prices. Check symbol. The number of requests may reached its limit");
                        return;
                    }
                    stock.currPrice = Double.Parse(json["results"][stock.symbol]["price"].ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Error at getting prices. Check key and symbol. The number of requests may reached its limit");
                    return;
                }
                if (stock.currPrice < stock.buyPrice || stock.currPrice > stock. sellPrice)
                {

                    if (stocksAlert.Contains(stock) == false)
                    {
                        stocksAlert.Add(stock);
                        this.Notify();
                    }
                }
                else
                {
                    stocksAlert.Remove(stock);
                }
                Thread.Sleep(this.sleepTime);
            }
        }

        /// <summary>
        /// Returns the last stock added in the StockObserver.stocksAlert list
        /// </summary>
        /// <returns></returns>
        public Stock GetLastStock()
        {
            return this.stocksAlert.Last();
        }

        /// <summary>
        /// Add a new stock in StockObserver.stocksAlert list
        /// </summary>
        /// <param name="observer"></param>
        public void Add(IObserver observer)
        {
            this.obsList.Add(observer);
        }

        /// <summary>
        /// Remove the stock in the parameters from StockObserver.stocksAlert list
        /// </summary>
        /// <param name="observer"></param>
        public void Remove(IObserver observer)
        {
            this.obsList.Remove(observer);
        }

        /// <summary>
        /// Call the IObserver.Update() for every observer in StockObserver.obsList. This method is part of the Oberver pattern
        /// </summary>
        public void Notify()
        {
            foreach (IObserver observer in obsList)
            {
                observer.Update();
            }
        }
    }
}
