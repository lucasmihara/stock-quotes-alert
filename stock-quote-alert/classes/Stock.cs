using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace stock_quote_alert.classes
{
    internal class Stock
    {
        public string symbol { get; set; }
        public double currPrice { get; set; }
        public double buyPrice { get; set; }
        public double sellPrice { get; set; }

        public Stock(string symbol, double buyPrice, double sellPrice)
        {
            this.symbol = symbol;  
            this.buyPrice = buyPrice;
            this.sellPrice = sellPrice;
        }
    }
}
