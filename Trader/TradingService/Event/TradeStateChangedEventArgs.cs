using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Enum;

namespace Trader.TradingService.Event
{
    public class TradeStateChangedEventArgs : EventArgs
    {
        public State State { get; set; }
    }
}
