using Binance;
using Binance.Cache;
using Binance.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trader.Utility.LogManager.Data;
using Trader.Utility.SettingsManager.Enum;
using Trader.TradingService.Enum;
using Trader.TradingService.Event;
using Trader.TradingService.Utility;
using Trader.Utility.SettingsManager.Manager;
using Trader.Utility.LogManager.Manager;
using Trader.Utility;

namespace Trader.TradingService.Service
{
    public class ThresholdTradingService : TradingService
    {
        #region Constants

        public const string NAME = "thresholdTrading";

        #endregion

        #region Private Variables

        private Timer tradeTimer;

        private TradeSide tradeSide = TradeSide.None;

        private decimal priceLastBought = 0;
        private decimal priceLastSold = 0;

        private decimal totalBTC = 0;

        private bool isOrderWaiting = false;
        private long waitingOrderId = -1;

        private DateTime dateLastSold;

        #endregion

        #region Properties

        public int TradeCheckInterval { get; set; } // in seconds
        public int CheckTimeoutForBuying { get; set; } // in minutes

        public decimal BuyDifferenceThreshold { get; set; } // in BTC
        public decimal SellDifferenceThreshold { get; set; } // in BTC
        #endregion

        public override void Initialize(bool subscribeSymbolsAutomatically = true)
        {
            base.Initialize(subscribeSymbolsAutomatically);

            TradeCheckInterval = SettingsManager.Instance.Get(ThresholdTradingPropertyName.TRADING_CHECK_INTERVAL, ThresholdTradingDefaults.TRADING_CHECK_INTERVAL);
            CheckTimeoutForBuying = SettingsManager.Instance.Get(ThresholdTradingPropertyName.CHECK_TIMEOUT_FOR_BUYING, ThresholdTradingDefaults.CHECK_TIMEOUT_FOR_BUYING);
            BuyDifferenceThreshold = SettingsManager.Instance.Get(ThresholdTradingPropertyName.BUY_DIFFERENCE_THRESHOLD, ThresholdTradingDefaults.BUY_DIFFERENCE_THRESHOLD);
            SellDifferenceThreshold = SettingsManager.Instance.Get(ThresholdTradingPropertyName.SELL_DIFFERENCE_THRESHOLD, ThresholdTradingDefaults.SELL_DIFFERENCE_THRESHOLD);

            tradeTimer = new Timer()
            {
                Interval = TradeCheckInterval * 1000
            };
            tradeTimer.Tick += TradeTimer_Tick;
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
            }

            tradeTimer.Start();
        }

        public override void StopTrading()
        {
            base.StopTrading();

            tradeTimer.Stop();
        }

        private void TradeTimer_Tick(object sender, EventArgs e)
        {
            MakeTrade();
        }

        private async void MakeTrade()
        {
            if (isOrderWaiting)
            {
                // if is order was waiting check it for filling status
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

                    if (order.Side == OrderSide.Buy)
                        tradeSide = TradeSide.Sell;
                    else if (order.Side == OrderSide.Sell)
                    {
                        tradeSide = TradeSide.Buy;
                        dateLastSold = DateTime.Now; // set dateLastSold to now because from now on service will be look for buy
                    }

                    ResetValuesAfterTrade();

                    await Task.Delay(1000);
                }
                else if (order.Status == OrderStatus.Canceled || order.Status == OrderStatus.Expired || order.Status == OrderStatus.Rejected)
                {
                    isOrderWaiting = false;
                }
            }

            if (!isOrderWaiting)
            {
                // there is no open order so lets trade
                var lastTrade = LogManager.Instance.GetLastTradeLog();
                if (lastTrade != null)
                {
                    if (lastTrade.Side == TradeSide.Buy)
                    {
                        // time to sell
                        CheckForSell(lastTrade);
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
                        var symbol = TradingServiceHelper.GetSymbolByName(base.Symbol);

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
                        CheckForSell();
                    else if (tradeSide == TradeSide.Buy)
                        CheckForBuy();
                }
            }
        }

        private async void CheckForSell(TradeLog lastTrade = null)
        {
            if (SellDifferenceThreshold <= 0)
                throw new Exception("Sell difference threshold is " + SellDifferenceThreshold + " in TradingService!");

            var lastSymbolLog = LogManager.Instance.GetLastSymbolLog();
            if (lastSymbolLog != null)
            {
                decimal difference = 0;

                if (lastTrade != null)  // if last trade made in the application
                {
                    difference = lastSymbolLog.Value - lastTrade.Price;
                }
                else // last trade is null so last trade must be made in the site
                {
                    if (priceLastBought == 0)
                        priceLastBought = await GetLastBoughtPrice();

                    if (priceLastBought != 0)
                        difference = lastSymbolLog.Value - priceLastBought;
                }

                FileLogger.Instance.WriteLog("ThresholdTrading: Sell Check | Price: " + lastSymbolLog.Value + " | Difference: " + difference);

                if (difference != 0 && difference >= SellDifferenceThreshold)
                {
                    // difference threshold is exceeded, time to give LIMIT order
                    decimal amount = 0;
                    if (lastTrade != null)
                        amount = lastTrade.Amount * 0.9995m;
                    else
                    {
                        var lastBoughtOrder = await GetLastBoughtOrder();
                        amount = lastBoughtOrder.OriginalQuantity * 0.9995m;
                    }

                    amount = decimal.Parse(amount.ToString("#.##"));

                    FileLogger.Instance.WriteLog("ThresholdTrading: Order Creating : Sell | Quantity: " + amount + " Price: " + lastSymbolLog.Value);
                    var order = await Api.PlaceAsync(new LimitOrder(User)
                    {
                        Symbol = Symbol,
                        Side = OrderSide.Sell,
                        Quantity = amount,
                        Price = lastSymbolLog.Value
                    });
                    FileLogger.Instance.WriteLog("Order Id : " + order.Id.ToString());

                    isOrderWaiting = true;
                    waitingOrderId = order.Id;
                }
            }
        }

        private async void CheckForBuy(TradeLog lastTrade = null)
        {
            if (BuyDifferenceThreshold <= 0)
                throw new Exception("Buy difference threshold is " + BuyDifferenceThreshold + " in TradingService!");

            var lastSymbolLog = LogManager.Instance.GetLastSymbolLog();
            if (lastSymbolLog != null)
            {
                decimal difference = 0;

                if (lastTrade != null)  // last trade must be made in the application
                {
                    difference = lastSymbolLog.Value - lastTrade.Price;
                    totalBTC = lastTrade.Price * lastTrade.Amount;
                }
                else // last trade is null so last trade must be made in the site
                {
                    if (priceLastSold == 0 || totalBTC == 0)
                    {
                        var lastSoldOrder = await GetLastSoldOrder();
                        if (lastSoldOrder != null)
                        {
                            priceLastSold = lastSoldOrder.Price;
                            totalBTC = lastSoldOrder.Price * lastSoldOrder.OriginalQuantity;
                        }
                    }

                    if (priceLastSold != 0 && totalBTC != 0)
                        difference = lastSymbolLog.Value - priceLastSold;
                }

                FileLogger.Instance.WriteLog("ThresholdTrading: Buy Check | Price: " + lastSymbolLog.Value + " | Difference: " + difference);

                // difference threshold is exceeded, time to give LIMIT order
                if (difference != 0 && difference <= -BuyDifferenceThreshold)
                {
                    decimal amount = (totalBTC * 0.999m) / lastSymbolLog.Value;
                    amount = decimal.Parse(amount.ToString("#.##"));

                    FileLogger.Instance.WriteLog("ThresholdTrading: Order Creating : Buy | Quantity: " + amount + " Price: " + lastSymbolLog.Value);
                    var order = await Api.PlaceAsync(new LimitOrder(User)
                    {
                        Symbol = Symbol,
                        Side = OrderSide.Buy,
                        Quantity = amount,
                        Price = lastSymbolLog.Value
                    });
                    FileLogger.Instance.WriteLog("Order Id : " + order.Id.ToString());

                    isOrderWaiting = true;
                    waitingOrderId = order.Id;
                }
                else if (difference != 0)
                {
                    if ((DateTime.Now - dateLastSold).TotalMinutes >= CheckTimeoutForBuying)
                    {
                        // time is out for checking for buying so lets control graph
                    }
                }
            }
        }

        #region Utility Functions

        private void ResetValuesAfterTrade()
        {
            isOrderWaiting = false;
            priceLastSold = 0;
            priceLastBought = 0;
            totalBTC = 0;
        }

        #endregion
    }
}
