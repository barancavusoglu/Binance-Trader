using Binance;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trader.TradingService.Enum;
using Trader.TradingService.Utility;
using Trader.Utility;
using Trader.Utility.LogManager.Data;
using Trader.Utility.LogManager.Manager;
using Trader.Utility.SettingsManager.Manager;

namespace Trader.TradingService.Service
{
    public class HitNRunTradingService : TradingService
    {
        #region Constants

        public const string NAME = "hitNRunTrading";
        private const int ORDER_CHECK_INTERVAL = 10 * 1000; // in ms

        #endregion

        #region Private Variables

        private Timer stablenessCheckTimer;
        private Timer orderCheckTimer;

        private TradeSide tradeSide = TradeSide.None;

        private decimal totalUSD = 0;
        private decimal priceLastSold = 0;

        private bool isOrderWaiting = false;
        private long waitingOrderId = -1;

        private DateTime dateLastSold;

        #endregion

        #region Properties

        public int StablenessCheckInterval { get; set; } // in minutes
        public int StablenessIndicatorThreshold { get; set; } // in usd

        public decimal SellDifferenceThreshold { get; set; } // in usd

        #endregion

        public override void Initialize(bool subscribeSymbolsAutomatically = true)
        {
            base.Initialize(subscribeSymbolsAutomatically);

            StablenessCheckInterval = SettingsManager.Instance.Get(HitNRunTradingPropertyName.STABLENESS_CHECK_INVERTVAL, HitNRunTradingDefaults.STABLENESS_CHECK_INTERVAL);
            StablenessIndicatorThreshold = SettingsManager.Instance.Get(HitNRunTradingPropertyName.STABLENESS_INDICATOR_THRESHOLD, HitNRunTradingDefaults.STABLENESS_INDICATOR_THRESHOD);
            SellDifferenceThreshold = SettingsManager.Instance.Get(HitNRunTradingPropertyName.SELL_DIFFERENCE_THRESHOLD, HitNRunTradingDefaults.SELL_DIFFERENCE_THRESHOLD);

            SubscribeSymbols();

            stablenessCheckTimer = new Timer()
            {
                Interval = StablenessCheckInterval * 1000 * 60
            };
            stablenessCheckTimer.Tick += StablenessCheckTimer_Tick;

            orderCheckTimer = new Timer()
            {
                Interval = ORDER_CHECK_INTERVAL
            };
            orderCheckTimer.Tick += OrderCheckTimer_TickAsync;
        }

        public override async void StartTrading()
        {
            base.StartTrading();

            // first check for open orders (for to be sure about open orders)
            var orders = await Api.GetOpenOrdersAsync(User);
            if (orders != null && orders.Count() > 0)
            {
                // there is an open order
                isOrderWaiting = true;
                waitingOrderId = orders.First().Id;

                orderCheckTimer.Start();
            }
            else
            {
                MakeTrade();
                stablenessCheckTimer.Start();
            }
        }

        public override void StopTrading()
        {
            base.StopTrading();

            stablenessCheckTimer.Stop();
            orderCheckTimer.Stop();
        }

        private void StablenessCheckTimer_Tick(object sender, EventArgs e)
        {
            MakeTrade();

#if TRADE_TEST
            stablenessCheckTimer.Stop();
#endif
        }

        private async void OrderCheckTimer_TickAsync(object sender, EventArgs e)
        {
            if (isOrderWaiting)
            {
                FileLogger.Instance.WriteLog($"Hit&RunTrading: Order Check | Order ID: {waitingOrderId}");

                // if order was waiting check it for filling status
                var order = await Api.GetOrderAsync(User, Symbol, waitingOrderId);
                if (order.Status == OrderStatus.Filled)
                {
                    LogManager.Instance.AddLog(new TradeLog()
                    {
                        Amount = order.OriginalQuantity,
                        Price = order.Price,
                        Side = order.Side == OrderSide.Buy ? TradeSide.Buy : TradeSide.Sell,
                        Symbol = order.Symbol
                    });

                    FileLogger.Instance.WriteLog($"Hit&RunTrading: Order Check | Order ID: {waitingOrderId} has been filled!");

                    if (order.Side == OrderSide.Buy)
                        tradeSide = TradeSide.Sell;
                    else if (order.Side == OrderSide.Sell)
                    {
                        tradeSide = TradeSide.Buy;
                        dateLastSold = DateTime.Now;
                    }

                    ResetValuesAfterTrade();
                    orderCheckTimer.Stop();

                    await Task.Delay(2000);

                    if (tradeSide == TradeSide.Sell)
                        MakeTrade();

                    stablenessCheckTimer.Start();
                }
                else if (order.Status == OrderStatus.Canceled || order.Status == OrderStatus.Expired || order.Status == OrderStatus.Rejected)
                {
                    isOrderWaiting = false;
                }
            }

#if TRADE_TEST
            orderCheckTimer.Stop();
#endif
        }

        private async void MakeTrade()
        {
            if (!isOrderWaiting)
            {
                var lastTrade = LogManager.Instance.GetLastTradeLog(Symbol);
                if (lastTrade != null)
                {
                    if (lastTrade.Side == TradeSide.Buy)
                    {
                        // time to create sell order
                        CreateSellOrderAsync(lastTrade);
                    }
                    else if (lastTrade.Side == TradeSide.Sell)
                    {
                        // time to buy
                        CheckForBuy(lastTrade);
                    }
                }
                else
                {
                    if (tradeSide == TradeSide.None) // if TradeSide is none decide tradeSide by checking base and quote asset value
                    {
                        var account = await Api.GetAccountInfoAsync(User);
                        var symbol = TradingServiceHelper.GetSymbolByName(Symbol);

                        decimal baseAssetValue = 0;
                        decimal quoteAssetValue = 0;

                        foreach (var balance in account.Balances)
                        {
                            if (balance.Asset == symbol.BaseAsset)
                                baseAssetValue = balance.Free;
                            else if (balance.Asset == symbol.QuoteAsset) // MUST BE BTC
                                quoteAssetValue = balance.Free;
                        }

                        var price = await Api.GetPriceAsync(symbol);
                        var baseAssetValueInQuoteAsset = price.Value * baseAssetValue; // base asset's value in quote asset currency

                        if (baseAssetValueInQuoteAsset > quoteAssetValue)
                            tradeSide = TradeSide.Sell;
                        else
                        {
                            tradeSide = TradeSide.Buy;
                            dateLastSold = DateTime.Now; // set dateLastSold to now because from now on service will be look for buy
                        }
                    }

                    if (tradeSide == TradeSide.Sell)
                        throw new Exception($"You must start HitNRun trading with quote asset! ({SymbolObject.QuoteAsset.Symbol})");
                    else if (tradeSide == TradeSide.Buy)
                        CheckForBuy();
                }
            }
        }

        private async void CreateSellOrderAsync(TradeLog lastTrade)
        {
            if (SellDifferenceThreshold <= 0)
                throw new Exception("Hit&RunTrading: SellDifferenceThreshold is less then zero! " + SellDifferenceThreshold);

            decimal amountToSell = await GetSymbolBalance(BaseAsset); //bnb 9995
            amountToSell = amountToSell.RoundTo(Decimals);

            FileLogger.Instance.WriteLog("Hit&RunTrading: Order Creating : Sell | Quantity: " + amountToSell + " Price: " + lastTrade.Price + SellDifferenceThreshold);
            var order = await Api.PlaceAsync(new LimitOrder(User)
            {
                Symbol = Symbol,
                Side = OrderSide.Sell,
                Quantity = amountToSell,
                Price = lastTrade.Price + SellDifferenceThreshold
            });
            FileLogger.Instance.WriteLog("Order Id : " + order.Id.ToString());

            isOrderWaiting = true;
            waitingOrderId = order.Id;

            stablenessCheckTimer.Stop();
            orderCheckTimer.Start();
        }

        private async void CheckForBuy(TradeLog lastTrade = null)
        {
            //if (lastTrade != null)  // last trade must be made in the application
            //    totalUSD = lastTrade.Price * lastTrade.Amount;
            //else // last trade is null so last trade must be made in the site
            //{
            //    if (totalUSD == 0)
            //    {
            //        //var lastSoldOrder = await GetLastSoldOrder();
            //        //if (lastSoldOrder != null)
            //        //{
            //        //    priceLastSold = lastSoldOrder.Price;
            //        //    totalUSD = lastSoldOrder.Price * lastSoldOrder.OriginalQuantity;
            //        //}
            //        totalUSD = await GetUSDTBalance();
            //    }
            //}

            if (totalUSD == 0)
                totalUSD = await GetUSDTBalance();

            totalUSD = totalUSD.RoundTo(2);

            var dateEnd = DateTime.UtcNow;
            var dateStart = dateEnd.AddMinutes(-30);

            var candlesticks = await Api.GetCandlesticksAsync(Symbol, CandlestickInterval.Minutes_3, dateStart, dateEnd);

            var minPrice = decimal.MaxValue;
            var maxPrice = decimal.MinValue;

            foreach (var candlestick in candlesticks)
            {
                var averageCandlestickPrice = candlestick.High + candlestick.Low / 2;

                if (averageCandlestickPrice < minPrice)
                    minPrice = averageCandlestickPrice;

                if (averageCandlestickPrice > maxPrice)
                    maxPrice = averageCandlestickPrice;
            }
            var difference = maxPrice - minPrice;

            FileLogger.Instance.WriteLog($"HitNRunTrading: Stableness Check | Difference: {difference}");

            var lastPrice = LogManager.Instance.GetLastSymbolLog();
            if (difference <= StablenessIndicatorThreshold && lastPrice != null && lastPrice.Value != 0)
            {
                // its stable, time to buy
                decimal amountToBuy = totalUSD / lastPrice.Value;
                amountToBuy = amountToBuy.RoundTo(Decimals);

                FileLogger.Instance.WriteLog($"HitNRunTrading: Stableness Check | Difference is less than StablenessIndicatorThreshold! Time to buy!");
                FileLogger.Instance.WriteLog("HitNRunTrading: Order Creating : Buy | Quantity: " + amountToBuy + " Price: " + lastPrice.Value);
                var order = await Api.PlaceAsync(new LimitOrder(User)
                {
                    Symbol = Symbol,
                    Side = OrderSide.Buy,
                    Quantity = amountToBuy,
                    Price = lastPrice.Value
                });
                FileLogger.Instance.WriteLog("Order Id : " + order.Id.ToString());

                isOrderWaiting = true;
                waitingOrderId = order.Id;

                stablenessCheckTimer.Stop();
                orderCheckTimer.Start();
            }

        }


        #region Utility Functions

        private void ResetValuesAfterTrade()
        {
            isOrderWaiting = false;
            totalUSD = 0;
        }

        #endregion
    }
}
