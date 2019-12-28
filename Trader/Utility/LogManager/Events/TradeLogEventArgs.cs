using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Utility.LogManager.Data;

namespace Trader.Utility.LogManager.Events
{
    public class TradeLogEventArgs : EventArgs
    {
        public TradeLog TradeLog { get; set; }
    }
}
