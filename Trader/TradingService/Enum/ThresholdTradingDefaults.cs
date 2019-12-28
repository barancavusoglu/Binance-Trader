using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.TradingService.Enum
{
    public class ThresholdTradingDefaults
    {
        public const decimal BUY_DIFFERENCE_THRESHOLD = 0.000005M; // in BTC value
        public const decimal SELL_DIFFERENCE_THRESHOLD = 0.000005M; // in BTC value
        public const int TRADING_CHECK_INTERVAL = 10; // in seconds
        public const int CHECK_TIMEOUT_FOR_BUYING = 30; // in minutes
    }
}
