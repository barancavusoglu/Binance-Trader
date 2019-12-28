using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.TradingService.Enum
{
    public class HitNRunTradingDefaults
    {
        public const int STABLENESS_CHECK_INTERVAL = 10; // in minutes
        public const int STABLENESS_INDICATOR_THRESHOD = 10; // in usd value
        public const decimal SELL_DIFFERENCE_THRESHOLD = 20; // in usd value
    }
}
