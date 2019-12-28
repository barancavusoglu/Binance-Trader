using Binance;
using System;
using System.Linq;
using System.Threading.Tasks;
using Trader.Utility;

namespace Trader.TradingService.Utility
{
    public class TradingServiceHelper
    {
        public static Symbol GetSymbolByName(string name)
        {
            var symbol = Symbol.Cache.Get(name);

            //if (symbol == null)
            //{
            //    var symbols = await new BinanceApi().GetSymbolsAsync();
            //    foreach (var symbol_ in symbols)
            //    {
            //        if (symbol_.ToString() == name)
            //            return symbol_;
            //    }
            //}


            return symbol;
        }

        public static async Task<decimal> GetPriceByDate(string symbol, DateTime date)
        {
            var api = new BinanceApi();
            var candleSticks = await api.GetCandlesticksAsync(symbol, CandlestickInterval.Minute, date, date.AddMinutes(1));
            if (candleSticks != null && candleSticks.Any())
                return ((candleSticks.First().High + candleSticks.First().Low) / 2m).RoundTo(8);
            else
                return 0m;
        }
    }
}
