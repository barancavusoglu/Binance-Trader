using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Enum
{
    public enum State
    {
        Ready,
        Processing,
        WaitingForBuy,
        WaitingForSell,
        WaitingForOpenBuyOrder,
        WaitingForOpenSellOrder
    }
}
