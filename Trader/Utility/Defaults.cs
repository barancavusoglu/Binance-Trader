using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Utility
{
    public class Defaults
    {
        public const string ENCRYPTION_SALT = "***_-xxxxxxx-_***";

        public const string TRADING_SYMBOL = "BTCUSDT";
        public const int DECIMALS = 6;

        public const string TRADING_TYPE = "thresholdTrading";

        public static Color Green => Color.FromArgb(115, 201, 33);
        public static Color Red => Color.FromArgb(202, 44, 120);
    }
}
