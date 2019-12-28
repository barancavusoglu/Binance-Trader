namespace Trader.TradingService.Enum
{
    public class ChartFollowTradingPropertyName
    {
        public static string TRADING_CHECK_INTERVAL => "chartFollowTrading.tradingCheckInterval";
        public static string INCREASE_INDICATOR_THRESHOLD => "chartFollowTrading.increaseIndicatorThreshold";
        public static string DECREASE_INDICATOR_THRESHOLD => "chartFollowTrading.decreaseIndicatorThreshold";

        public static string MIN_BUY_DIFFERENCE_THRESHOLD => "chartFollowTrading.minBuyDifferenceThreshold";
        public static string MIN_SELL_DIFFERENCE_THRESHOLD => "chartFollowTrading.minSellDifferenceThreshold";

        public static string STOP_LOSS_THRESHOLD => "chartFollowTrading.stopLossThreshold";
        public static string STOP_LOSS_AFTER_MINUTES => "chartFollowTrading.stopLossAfterMinutes";
        public static string LOSS_BUY_PERCENTAGE => "chartFollowTrading.lossBuyPercentage";
        public static string LOSS_BUY_DAYS => "chartFollowTrading.lossBuyDays";
    }
}
