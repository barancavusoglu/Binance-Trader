using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Utility
{
    public static class GlobalSettings
    {
        public static DateTime StartDate;

        public const string LOG_TRADE_FILENAME_PREFIX = "TradeLog_";
        public const string LOG_SYMBOL_FILENAME_PREFIX = "SymbolLog_";

        public static string LOG_SYMBOL_FILEPATH => Path.GetDirectoryName(LOG_SYMBOL_FILENAME);
        public static string LOG_TRADE_FILEPATH => Path.GetDirectoryName(LOG_TRADE_FILENAME);

        public static string LOG_TRADE_FILENAME => Path.Combine("TradeLogs", LOG_TRADE_FILENAME_PREFIX + StartDate.ToString("MM.dd.yyyy_HH.mm") + ".txt");
        public static string LOG_SYMBOL_FILENAME => Path.Combine("SymbolLogs", LOG_SYMBOL_FILENAME_PREFIX + StartDate.ToString("MM.dd.yyyy_HH.mm") + ".txt");
    }
}
