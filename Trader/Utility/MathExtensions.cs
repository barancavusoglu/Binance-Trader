using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Utility
{
    public static class MathExtensions
    {
        public static decimal RoundTo(this decimal value, int decimals)
        {
            var multiplier = (decimal)Math.Pow(10, decimals);
            return Math.Truncate(value * multiplier) / multiplier;
        }
    }
}
