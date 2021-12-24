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
    internal class StockObserver : IObservable
    {
        private string key;
        private int sleepTime = 30000; // 900000

        private List<IObserver> obsList = new List<IObserver> ();

        private List<Stock> stocksAlert = new List<Stock>();
        public StockObserver(string key)
        {
            this.key = key;
        }

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

        public Stock GetLastStock()
        {
            return this.stocksAlert.Last();
        }

        public void Add(IObserver observer)
        {
            this.obsList.Add(observer);
        }

        public void Remove(IObserver observer)
        {
            this.obsList.Remove(observer);
        }
        public void Notify()
        {
            foreach (IObserver observer in obsList)
            {
                observer.Update();
            }
        }
    }
}
