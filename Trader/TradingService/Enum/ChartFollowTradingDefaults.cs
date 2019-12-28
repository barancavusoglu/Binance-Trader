using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.TradingService.Enum
{
    public class ChartFollowTradingDefaults
    {
        public const int TRADING_CHECK_INTERVAL = 3000; // in milliseconds
        public const decimal INCREASE_INDICATOR_THRESHOLD = 0.00001M; // in BTC value
        public const decimal DECREASE_INDICATOR_THRESHOLD = 0.00001M; // in BTC value
        public const decimal MIN_BUY_DIFFERENCE_THRESHOLD = 0.000008M; // in BTC value
        public const decimal MIN_SELL_DIFFERENCE_THRESHOLD = 0.00008M; // in BTC value

        public const decimal DOUBLESIDED_INCREASE_INDICATOR_THRESHOLD = 0.1M; // in USDT value
        public const decimal DOUBLESIDED_DECREASE_INDICATOR_THRESHOLD = 0.1M; // in USDT value
        public const decimal DOUBLESIDED_MIN_BUY_DIFFERENCE_THRESHOLD = 0.08M; // in USDT value
        public const decimal DOUBLESIDED_MIN_SELL_DIFFERENCE_THRESHOLD = 0.08M; // in USDT value

        public const int DOUBLESIDED_TIMEOUT_BUY_MINUTES = 45; // in minutes
        public const int DOUBLESIDED_TIMEOUT_SELL_MINUTES = 45; // in minutes

        public const bool DOUBLESIDED_ACTIVE = false;
    }
}
