using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.TradingService.Enum
{
    public class ThresholdTradingPropertyName
    {
        // threshold trading service
        public static string TRADING_CHECK_INTERVAL => "thresholdTrading.tradingCheckInterval";
        public static string CHECK_TIMEOUT_FOR_BUYING => "thresholdTrading.checkTimeOutForBuying";

        public static string BUY_DIFFERENCE_THRESHOLD => "thresholdTrading.buyDifferenceThreshold";
        public static string SELL_DIFFERENCE_THRESHOLD => "thresholdTrading.sellDifferenceThreshold";
    }
}
