using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Utility.LogManager.Data
{
    public class SymbolLog : Log
    {
        public string Symbol { get; set; }
        public decimal Value { get; set; }
    }
}
