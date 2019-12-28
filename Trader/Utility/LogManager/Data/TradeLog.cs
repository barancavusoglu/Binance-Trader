using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.TradingService.Enum;

namespace Trader.Utility.LogManager.Data
{
    public class TradeLog : Log
    {
        [Browsable(false)]
        public long OrderId { get; set; }
        public TradeSide Side { get; set; }
        public string Symbol { get; set; }
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
        public decimal Total => (Amount * Price).RoundTo(6);
    }
}
