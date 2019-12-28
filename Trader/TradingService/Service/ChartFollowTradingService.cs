using Binance;
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
    public class ChartFollowTradingService : TradingService
    {
        #region Constants

        public const string NAME = "chartFollowTrading";

        #endregion

        #region Private Variables

        private Timer tradeTimer;

        private TradeSide tradeSide = TradeSide.None;

        private decimal lastBuyPrice = 0;
        private decimal lastSellPrice = 0;

        private decimal maxPrice = int.MinValue;
        private decimal minPrice = int.MaxValue;

        private decimal totalDecrease = 0;
        private decimal totalIncrease = 0;

        private decimal totalMainFund = 0;

        private bool isOrderWaiting = false;
        private long waitingOrderId = -1;

        private DateTime? lastSellDate = null;
        private DateTime? lastBuyDate = null;

        private decimal tempIncreaseIndicatorThreshold; // in BTC
        private decimal tempDecreaseIndicatorThreshold; // in BTC

        private decimal tempMinimumBuyDifferenceThreshold; // in BTC
        private decimal tempMinimumSellDifferenceThreshold; // in BTC

        private decimal tempStopLossThreshold;
        private decimal tempLossBuyThreshold;

        private bool marketBuyContinue = false;

        #endregion

        #region Properties
        public int TradeCheckInterval { get; set; } // in milliseconds

        // main fund -> BTC
        public decimal IncreaseIndicatorThreshold { get; set; } // in BTC
        public decimal DecreaseIndicatorThreshold { get; set; } // in BTC

        public decimal MinimumBuyDifferenceThreshold { get; set; } // in BTC
        public decimal MinimumSellDifferenceThreshold { get; set; } // in BTC

        public decimal StopLossThreshold { get; set; }
        public decimal StopLossAfterMinutes { get; set; }

        public decimal LossBuyThreshold { get; set; }
        public decimal LossBuyMinutes { get; set; }

        #endregion

        public override async void Initialize(bool subscribeSymbolsAutomatically = true)
        {
            base.Initialize(false);

            // load properties
            TradeCheckInterval = SettingsManager.Instance.Get(ChartFollowTradingPropertyName.TRADING_CHECK_INTERVAL, ChartFollowTradingDefaults.TRADING_CHECK_INTERVAL);
            tempIncreaseIndicatorThreshold = IncreaseIndicatorThreshold = SettingsManager.Instance.Get(ChartFollowTradingPropertyName.INCREASE_INDICATOR_THRESHOLD, ChartFollowTradingDefaults.INCREASE_INDICATOR_THRESHOLD);
            tempDecreaseIndicatorThreshold = DecreaseIndicatorThreshold = SettingsManager.Instance.Get(ChartFollowTradingPropertyName.DECREASE_INDICATOR_THRESHOLD, ChartFollowTradingDefaults.DECREASE_INDICATOR_THRESHOLD);
            tempMinimumBuyDifferenceThreshold = MinimumBuyDifferenceThreshold = SettingsManager.Instance.Get(ChartFollowTradingPropertyName.MIN_BUY_DIFFERENCE_THRESHOLD, ChartFollowTradingDefaults.MIN_BUY_DIFFERENCE_THRESHOLD);
            tempMinimumSellDifferenceThreshold = MinimumSellDifferenceThreshold = SettingsManager.Instance.Get(ChartFollowTradingPropertyName.MIN_SELL_DIFFERENCE_THRESHOLD, ChartFollowTradingDefaults.MIN_SELL_DIFFERENCE_THRESHOLD);

            tempStopLossThreshold = StopLossThreshold = SettingsManager.Instance.Get(ChartFollowTradingPropertyName.STOP_LOSS_THRESHOLD, 0m);
            StopLossAfterMinutes = SettingsManager.Instance.Get(ChartFollowTradingPropertyName.STOP_LOSS_AFTER_MINUTES, 0m);
            tempLossBuyThreshold = LossBuyThreshold = SettingsManager.Instance.Get(ChartFollowTradingPropertyName.LOSS_BUY_PERCENTAGE, 0m);
            LossBuyMinutes = SettingsManager.Instance.Get(ChartFollowTradingPropertyName.LOSS_BUY_DAYS, 0m);

            SubscribeSymbols();

            tradeTimer = new Timer()
            {
                Interval = TradeCheckInterval
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
                foreach (var order in orders)
                {
                    if (order.Symbol == Symbol)
                    {
                        // there is an open order
                        isOrderWaiting = true;
                        waitingOrderId = order.Id;
                        break;
                    }
                }
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
            //tradeTimer.Stop();
            if (!marketBuyContinue)
            {
                MakeTrade();
            }
        }

        private async void MakeTrade()
        {
            if (isOrderWaiting)
            {
                // if is order was waiting check it for filling status
                var order = await Api.GetOrderAsync(User, Symbol, waitingOrderId);
                if (order.Status == OrderStatus.Filled)
                {
                    if (order.Type == Binance.OrderType.Limit)
                    {
                        LogManager.Instance.AddLog(new TradeLog()
                        {
                            Amount = order.ExecutedQuantity,
                            Price = order.Price,
                            Side = order.Side == OrderSide.Buy ? TradeSide.Buy : TradeSide.Sell,
                            Symbol = order.Symbol
                        });
                    }

                    if (order.Side == OrderSide.Buy)
                    {
                        tradeSide = TradeSide.Sell;
                        lastBuyDate = DateTime.Now;
                    }
                    else if (order.Side == OrderSide.Sell)
                    {
                        tradeSide = TradeSide.Buy;
                        lastSellDate = DateTime.Now;
                    }

                    ResetValuesAfterTrade();

                    await Task.Delay(500);
                }
                else if (order.Status == OrderStatus.Canceled || order.Status == OrderStatus.Expired || order.Status == OrderStatus.Rejected)
                {
                    isOrderWaiting = false;
                }
            }

            if (!isOrderWaiting)
            {
                // there is no open order so lets trade
                var lastTrade = LogManager.Instance.GetLastTradeLog(Symbol);
                if (lastTrade != null)
                {
                    if (lastTrade.Side == TradeSide.Buy)
                    {
                        // time to sell
                        lastBuyDate = lastTrade.Date;
                        lastBuyPrice = lastTrade.Price;
                        CheckForSell(lastTrade);
                    }
                    else if (lastTrade.Side == TradeSide.Sell)
                    {
                        // time to buy
                        lastSellDate = lastTrade.Date;
                        lastSellPrice = lastTrade.Price;
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
                        }
                    }

                    if (tradeSide == TradeSide.Sell)
                    {
                        CheckForSell();
                    }
                    else if (tradeSide == TradeSide.Buy)
                    {
                        CheckForBuy();
                    }
                }
            }
        }

        private async void CheckForSell(TradeLog lastTrade = null)
        {
            if (tempDecreaseIndicatorThreshold <= 0)
                throw new Exception("Decrease Indicator Threshold  is" + tempDecreaseIndicatorThreshold + " in ChartFollowTradingService!");

            if (lastTrade != null)  // if last trade made in the application
            {
                lastBuyPrice = lastTrade.Price;
            }
            else // last trade is null so last trade must be made in the site
            {
                if (lastBuyPrice == 0)
                    lastBuyPrice = await GetLastBoughtPrice(Symbol);
            }

            var lastPrice = LogManager.Instance.GetLastSymbolLog(Symbol, TradeCheckInterval);
            if (lastPrice != null)
            {
                if (UnitType == UnitType.Percentage)
                {
                    tempIncreaseIndicatorThreshold = (IncreaseIndicatorThreshold / 100m) * lastPrice.Value;
                    tempDecreaseIndicatorThreshold = (DecreaseIndicatorThreshold / 100m) * lastPrice.Value;
                    tempMinimumBuyDifferenceThreshold = (MinimumBuyDifferenceThreshold / 100m) * lastPrice.Value;
                    tempMinimumSellDifferenceThreshold = (MinimumSellDifferenceThreshold / 100m) * lastPrice.Value;
                    tempStopLossThreshold = (StopLossThreshold / 100m) * lastPrice.Value;
                    tempLossBuyThreshold = (LossBuyThreshold / 100m) * lastPrice.Value;
                }

                var difference = lastPrice.Value - lastBuyPrice;
                FileLogger.Instance.WriteLog("ChartFollowTrading: Sell Check | Price: " + lastPrice.Value + " | Difference: " + difference);

                // look for highest price
                if (lastPrice.Value > maxPrice)
                    maxPrice = lastPrice.Value;

                totalDecrease = maxPrice - lastPrice.Value;

                FileLogger.Instance.WriteLog("ChartFollowTrading: Sell Check | MaxPrice: " + maxPrice + " | Last Bought Price: " + lastBuyPrice);
                FileLogger.Instance.WriteLog("ChartFollowTrading: Sell Check | TotalDecrease: " + totalDecrease);

                if (totalDecrease >= tempDecreaseIndicatorThreshold && difference >= tempMinimumSellDifferenceThreshold)
                {
                    // decreasing has been started, lets sell
                    decimal amountToSell = await GetSymbolBalance(BaseAsset); //bnb 9995
                    amountToSell = amountToSell.RoundTo(Decimals);

                    FileLogger.Instance.WriteLog("ChartFollowTrading: Order Creating : Sell | Quantity: " + amountToSell + " Price: " + lastPrice.Value);
                    Order order = null;
                    if (OrderType == Enum.OrderType.Limit)
                    {
                        order = await Api.PlaceAsync(new LimitOrder(User)
                        {
                            Symbol = Symbol,
                            Side = OrderSide.Sell,
                            Quantity = amountToSell,
                            Price = lastPrice.Value
                        });
                    }
                    else if (OrderType == Enum.OrderType.Market)
                    {
                        order = await Api.PlaceAsync(new MarketOrder(User)
                        {
                            Quantity = amountToSell,
                            Side = OrderSide.Sell,
                            Symbol = Symbol
                        });

                        var totalPrice = 0m;
                        var fills = order.Fills.ToList();
                        foreach (var fill in fills)
                            totalPrice += fill.Price * fill.Quantity;

                        var averageOrderPrice = totalPrice / amountToSell;

                        LogManager.Instance.AddLog(new TradeLog()
                        {
                            Amount = amountToSell,
                            Price = averageOrderPrice,
                            Side = order.Side == OrderSide.Buy ? TradeSide.Buy : TradeSide.Sell,
                            Symbol = order.Symbol
                        });

                        FileLogger.Instance.WriteLog("Sold from Market at Price Average: " + averageOrderPrice);
                    }

                    FileLogger.Instance.WriteLog("Order Id : " + order.Id.ToString());

                    isOrderWaiting = true;
                    waitingOrderId = order.Id;
                }
                else if (tempStopLossThreshold != 0)
                {
                    if (lastBuyDate != null && Math.Abs(maxPrice - lastPrice.Value) >= tempStopLossThreshold && (DateTime.Now - lastBuyDate).Value.TotalMinutes >= (double)StopLossAfterMinutes)
                    {
                        // decreasing has been started, lets sell
                        decimal amountToSell = await GetSymbolBalance(BaseAsset); //bnb 9995
                        amountToSell = amountToSell.RoundTo(Decimals);

                        FileLogger.Instance.WriteLog("ChartFollowTrading: Sell Check | Loss from MaxPrice: " + Math.Abs(maxPrice - lastPrice.Value));
                        FileLogger.Instance.WriteLog("ChartFollowTrading: Order Creating : Stop Loss Sell | Quantity: " + amountToSell + " Price: " + lastPrice.Value);

                        Order order = null;
                        if (OrderType == Enum.OrderType.Limit)
                        {
                            order = await Api.PlaceAsync(new LimitOrder(User)
                            {
                                Symbol = Symbol,
                                Side = OrderSide.Sell,
                                Quantity = amountToSell,
                                Price = lastPrice.Value
                            });
                        }
                        else if (OrderType == Enum.OrderType.Market)
                        {
                            order = await Api.PlaceAsync(new MarketOrder(User)
                            {
                                Quantity = amountToSell,
                                Side = OrderSide.Sell,
                                Symbol = Symbol
                            });

                            var totalPrice = 0m;
                            var fills = order.Fills.ToList();
                            foreach (var fill in fills)
                                totalPrice += fill.Price * fill.Quantity;

                            var averageOrderPrice = totalPrice / amountToSell;

                            LogManager.Instance.AddLog(new TradeLog()
                            {
                                Amount = amountToSell,
                                Price = averageOrderPrice,
                                Side = order.Side == OrderSide.Buy ? TradeSide.Buy : TradeSide.Sell,
                                Symbol = order.Symbol
                            });

                            FileLogger.Instance.WriteLog("StopLoss Sold from Market at Price Average: " + averageOrderPrice);
                        }
                        FileLogger.Instance.WriteLog("Order Id : " + order.Id.ToString());

                        isOrderWaiting = true;
                        waitingOrderId = order.Id;
                    }
                }
            }

            FileLogger.Instance.WriteLog("-------------------------------------------------------------", false);
        }

        private async void CheckForBuy(TradeLog lastTrade = null)
        {
            if (tempIncreaseIndicatorThreshold <= 0)
                throw new Exception("Increase Indicator Threshold  is" + tempIncreaseIndicatorThreshold + " in ChartFollowTradingService!");

            if (lastTrade != null)  // if last trade made in the application
            {
                lastSellPrice = lastTrade.Price;

                if (totalMainFund == 0)
                    totalMainFund = await GetSymbolBalance(QuoteAsset);//GetBTCBalance();
            }
            else // last trade is null so last trade must be made in the site
            {
                if (lastSellPrice == 0 || totalMainFund == 0)
                {
                    var lastSoldOrder = await GetLastSoldOrder(Symbol);
                    if (lastSoldOrder != null)
                    {
                        lastSellPrice = lastSoldOrder.Price;
                        totalMainFund = await GetSymbolBalance(QuoteAsset);//GetBTCBalance();
                    }
                }
            }

            var lastPrice = LogManager.Instance.GetLastSymbolLog(Symbol, TradeCheckInterval);
            if (lastPrice != null && totalMainFund != 0)
            {
                if (UnitType == UnitType.Percentage)
                {
                    tempIncreaseIndicatorThreshold = (IncreaseIndicatorThreshold / 100m) * lastPrice.Value;
                    tempDecreaseIndicatorThreshold = (DecreaseIndicatorThreshold / 100m) * lastPrice.Value;
                    tempMinimumBuyDifferenceThreshold = (MinimumBuyDifferenceThreshold / 100m) * lastPrice.Value;
                    tempMinimumSellDifferenceThreshold = (MinimumSellDifferenceThreshold / 100m) * lastPrice.Value;
                    tempStopLossThreshold = (StopLossThreshold / 100m) * lastPrice.Value;
                    tempLossBuyThreshold = (LossBuyThreshold / 100m) * lastPrice.Value;
                }

                var difference = lastSellPrice - lastPrice.Value;
                FileLogger.Instance.WriteLog("ChartFollowTrading: Buy Check | Price: " + lastPrice.Value + " | Difference: " + difference);

                if (lastPrice.Value < minPrice)
                    minPrice = lastPrice.Value;

                totalIncrease = lastPrice.Value - minPrice;

                FileLogger.Instance.WriteLog("ChartFollowTrading: Buy Check | MinPrice: " + minPrice + " | Last Sold Price: " + lastSellPrice);
                FileLogger.Instance.WriteLog("ChartFollowTrading: Buy Check | TotalIncrease: " + totalIncrease);

                if (totalIncrease >= tempIncreaseIndicatorThreshold && difference >= tempMinimumBuyDifferenceThreshold)
                {
                    // increasing has been started, lets buy
                    decimal amountToBuy = totalMainFund / lastPrice.Value;
                    amountToBuy = amountToBuy.RoundTo(Decimals);

                    FileLogger.Instance.WriteLog("ChartFollowTrading: Order Creating : Buy | Quantity: " + amountToBuy + " Price: " + lastPrice.Value);

                    Order order = null;
                    if (OrderType == Enum.OrderType.Limit)
                    {
                        order = await Api.PlaceAsync(new LimitOrder(User)
                        {
                            Symbol = Symbol,
                            Side = OrderSide.Buy,
                            Quantity = amountToBuy,
                            Price = lastPrice.Value
                        });
                        FileLogger.Instance.WriteLog("Order Id : " + order.Id.ToString());
                    }
                    else if (OrderType == Enum.OrderType.Market)
                    {
                        marketBuyContinue = true;

                        var success = false;
                        var buyAll = true;


                        while (!success || !buyAll)
                        {
                            var orderBookTop = await Api.GetOrderBookTopAsync(Symbol);
                            amountToBuy = totalMainFund / orderBookTop.Bid.Price;
                            amountToBuy = amountToBuy.RoundTo(Decimals);

                            if (amountToBuy > orderBookTop.Bid.Quantity)
                            {
                                amountToBuy = orderBookTop.Bid.Quantity;
                                buyAll = false;
                            }

                            totalMainFund -= amountToBuy * orderBookTop.Bid.Price;

                            try
                            {
                                order = await Api.PlaceAsync(new MarketOrder(User)
                                {
                                    Quantity = amountToBuy,
                                    Side = OrderSide.Buy,
                                    Symbol = Symbol
                                });

                                success = true;
                            }
                            catch (Exception ex)
                            {
                                success = false;
                            }

                            if (!success || !buyAll)
                                await Task.Delay(250);
                        }

                        var totalPrice = 0m;
                        var fills = order.Fills.ToList();
                        foreach (var fill in fills)
                            totalPrice += fill.Price * fill.Quantity;

                        var averageOrderPrice = totalPrice / amountToBuy;

                        LogManager.Instance.AddLog(new TradeLog()
                        {
                            Amount = amountToBuy,
                            Price = averageOrderPrice,
                            Side = order.Side == OrderSide.Buy ? TradeSide.Buy : TradeSide.Sell,
                            Symbol = order.Symbol
                        });

                        FileLogger.Instance.WriteLog("Buy from Market at Price Average: " + averageOrderPrice);
                    }
                    FileLogger.Instance.WriteLog("Order Id : " + order.Id.ToString());

                    isOrderWaiting = true;
                    waitingOrderId = order.Id;

                    marketBuyContinue = false;
                }
                else if (LossBuyThreshold != 0 && lastSellDate != null && (DateTime.Now - lastSellDate).Value.TotalMinutes >= (double)LossBuyMinutes)
                {
                    if (lastPrice.Value - minPrice >= tempLossBuyThreshold)
                    {
                        decimal amountToBuy = totalMainFund / lastPrice.Value;
                        amountToBuy = amountToBuy.RoundTo(Decimals);

                        FileLogger.Instance.WriteLog("ChartFollowTrading: Buy Check | Increase from Min Price: " + (lastPrice.Value - minPrice));
                        FileLogger.Instance.WriteLog("ChartFollowTrading: Order Creating : Loss Buy | Quantity: " + amountToBuy + " Price: " + lastPrice.Value);

                        Order order = null;
                        if (OrderType == Enum.OrderType.Limit)
                        {
                            order = await Api.PlaceAsync(new LimitOrder(User)
                            {
                                Symbol = Symbol,
                                Side = OrderSide.Buy,
                                Quantity = amountToBuy,
                                Price = lastPrice.Value
                            });
                            FileLogger.Instance.WriteLog("Order Id : " + order.Id.ToString());
                        }
                        else if (OrderType == Enum.OrderType.Market)
                        {
                            marketBuyContinue = true;

                            var success = false;
                            var buyAll = true;

                            while (!success || !buyAll)
                            {
                                var orderBookTop = await Api.GetOrderBookTopAsync(Symbol);
                                amountToBuy = totalMainFund / orderBookTop.Bid.Price;
                                amountToBuy = amountToBuy.RoundTo(Decimals);

                                if (amountToBuy > orderBookTop.Bid.Quantity)
                                {
                                    amountToBuy = orderBookTop.Bid.Quantity;
                                    buyAll = false;
                                }

                                totalMainFund -= amountToBuy * orderBookTop.Bid.Price;

                                try
                                {
                                    order = await Api.PlaceAsync(new MarketOrder(User)
                                    {
                                        Quantity = amountToBuy,
                                        Side = OrderSide.Buy,
                                        Symbol = Symbol
                                    });

                                    success = true;
                                }
                                catch (Exception ex)
                                {
                                    success = false;
                                }

                                if (!success || !buyAll)
                                    await Task.Delay(250);
                            }

                            marketBuyContinue = false;

                            var totalPrice = 0m;
                            var fills = order.Fills.ToList();
                            foreach (var fill in fills)
                                totalPrice += fill.Price * fill.Quantity;

                            var averageOrderPrice = totalPrice / amountToBuy;

                            LogManager.Instance.AddLog(new TradeLog()
                            {
                                Amount = amountToBuy,
                                Price = averageOrderPrice,
                                Side = order.Side == OrderSide.Buy ? TradeSide.Buy : TradeSide.Sell,
                                Symbol = order.Symbol
                            });

                            FileLogger.Instance.WriteLog("LossBuy from Market at Price Average: " + averageOrderPrice);
                        }
                        FileLogger.Instance.WriteLog("Order Id : " + order.Id.ToString());

                        isOrderWaiting = true;
                        waitingOrderId = order.Id;
                    }
                }
            }

            FileLogger.Instance.WriteLog("-------------------------------------------------------------", false);
        }

        #region Utility Functions

        private void ResetValuesAfterTrade()
        {
            isOrderWaiting = false;
            lastSellPrice = 0;
            lastBuyPrice = 0;
            totalMainFund = 0;
            totalDecrease = 0;
            totalIncrease = 0;
            maxPrice = decimal.MinValue;
            minPrice = decimal.MaxValue;
        }

        #endregion
    }
}
