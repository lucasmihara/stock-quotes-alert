using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stock_quote_alert.intefaces
{
    internal interface IObserver
    {
        void Update();
    }
}
