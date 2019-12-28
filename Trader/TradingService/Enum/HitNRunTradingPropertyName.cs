using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.TradingService.Enum
{
    public class HitNRunTradingPropertyName
    {
        // hit&run trading service
        public static string STABLENESS_CHECK_INVERTVAL => "hitNRunTrading.stablenessCheckInterval";
        public static string STABLENESS_INDICATOR_THRESHOLD => "hitNRunTrading.stablenessIndicatorThreshold";
        public static string SELL_DIFFERENCE_THRESHOLD => "hitNRunTrading.sellDifferenceThreshold";
    }
}
