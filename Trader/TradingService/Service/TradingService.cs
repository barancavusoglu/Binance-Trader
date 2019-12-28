using Binance;
using Binance.Cache;
using Binance.WebSocket;
using Quantum.Framework.GenericProperties.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trader.TradingService.Enum;
using Trader.TradingService.Utility;
using Trader.Utility;
using Trader.Utility.LogManager.Data;
using Trader.Utility.LogManager.Manager;
using Trader.Utility.SettingsManager.Enum;

namespace Trader.TradingService.Service
{
    public class TradingService
    {
        #region Private Variables

        private SymbolStatisticsWebSocketCache webSocketCache;

        #endregion

        #region Properties

        public bool Active { get; private set; }

        public BinanceApi Api { get; private set; }
        public BinanceApiUser User { get; private set; }
        public string Symbol { get; private set; }
        public Binance.Symbol SymbolObject => TradingServiceHelper.GetSymbolByName(Symbol);
        public int Decimals { get; set; }
        public UnitType UnitType { get; private set; }
        public Trader.TradingService.Enum.OrderType OrderType { get; private set; }

        public string BaseAsset => SymbolObject.BaseAsset;
        public string QuoteAsset => SymbolObject.QuoteAsset;

        #endregion

        public virtual async void Initialize(bool subscribeSymbolsAutomatically = true)
        {
            var apiKey = Trader.Utility.SettingsManager.Manager.SettingsManager.Instance.Get(SettingName.API_KEY, string.Empty);
            var secretKey = Trader.Utility.SettingsManager.Manager.SettingsManager.Instance.Get(SettingName.SECRET_KEY, string.Empty);

            if (!string.IsNullOrEmpty(apiKey))
                apiKey = EncryptionHelper.DecryptText(apiKey);
            if (!string.IsNullOrEmpty(secretKey))
                secretKey = EncryptionHelper.DecryptText(secretKey);

            Symbol = Trader.Utility.SettingsManager.Manager.SettingsManager.Instance.Get(SettingName.TRADING_SYMBOL, Defaults.TRADING_SYMBOL);
            Decimals = Trader.Utility.SettingsManager.Manager.SettingsManager.Instance.Get(SettingName.DECIMALS, Defaults.DECIMALS);

            var unitTypeStr = Trader.Utility.SettingsManager.Manager.SettingsManager.Instance.Get(SettingName.UNIT_TYPE, UnitType.Value.ToString());
            UnitType = (UnitType)System.Enum.Parse(typeof(UnitType), unitTypeStr, true);

            var orderTypeStr = Trader.Utility.SettingsManager.Manager.SettingsManager.Instance.Get(SettingName.ORDER_TYPE, Enum.OrderType.Limit.ToString());
            OrderType = (Enum.OrderType)System.Enum.Parse(typeof(Enum.OrderType), orderTypeStr, true);

            Api = new BinanceApi();

            if (subscribeSymbolsAutomatically)
                SubscribeSymbols();

            if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(secretKey))
                User = new BinanceApiUser(apiKey, secretKey);

            // get last orders
            LogManager.Instance.ClearTradeLogs();

            if (User != null)
            {
                try
                {
                    var ordersSymbol = await Api.GetOrdersAsync(User, Symbol, -1, 30);
                    foreach (var order in ordersSymbol)
                    {
                        if (order.Status == OrderStatus.Filled)
                        {
                            if (order.Type == Binance.OrderType.Limit)
                            {
                                LogManager.Instance.AddLog(new TradeLog()
                                {
                                    Amount = order.ExecutedQuantity,
                                    Date = order.Time.ToLocalTime(),
                                    OrderId = order.Id,
                                    Price = order.Price != 0 ? order.Price.RoundTo(6) : await TradingServiceHelper.GetPriceByDate(order.Symbol, order.Time),
                                    Side = order.Side == OrderSide.Buy ? TradeSide.Buy : TradeSide.Sell,
                                    Symbol = order.Symbol
                                }, false);
                            }
                            else
                            {
                                var price = 0m;
                                var trades = await Api.GetAccountTradesAsync(User, Symbol, 200);
                                foreach (var trade in trades)
                                {
                                    if (trade.OrderId == order.Id)
                                        price += trade.Price * trade.Quantity;
                                }
                                price = price / order.ExecutedQuantity;
                                price = price.RoundTo(6);

                                LogManager.Instance.AddLog(new TradeLog()
                                {
                                    Amount = order.ExecutedQuantity,
                                    Date = order.Time.ToLocalTime(),
                                    OrderId = order.Id,
                                    Price = price,
                                    Side = order.Side == OrderSide.Buy ? TradeSide.Buy : TradeSide.Sell,
                                    Symbol = order.Symbol
                                }, false);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        public virtual void Deinitialize()
        {
            if (webSocketCache != null)
                webSocketCache.Unsubscribe();
        }

        public virtual void StartTrading()
        {
            if (!Active)
            {
                Active = true;
            }
        }

        public virtual void StopTrading()
        {
            if (Active)
            {
                webSocketCache.Unsubscribe();
                Active = false;
            }
        }

        public void SubscribeSymbols()
        {
            webSocketCache = new SymbolStatisticsWebSocketCache();
            //webSocketCache.Error += WebSocketCache_Error;
            webSocketCache.Subscribe(ReceivedSymbol, new string[] { Symbol });
        }

        private void WebSocketCache_Error(object sender, ErrorEventArgs e)
        {
            MessageBox.Show("Web socket - Subscribe Symbols Error: " + e.Exception.Message);
        }

        private void ReceivedSymbol(SymbolStatisticsCacheEventArgs args)
        {
            foreach (var statistic in args.Statistics)
            {
                LogManager.Instance.AddLog(new SymbolLog()
                {
                    Symbol = statistic.Symbol,
                    Value = statistic.LastPrice
                });
            }
        }

        public virtual GenericPropertyCollection GetProperties()
        {
            return new GenericPropertyCollection()
            {
                new GenericProperty()
                {
                    ScopeName = TradingServicePropertyScope.GENERAL,
                    CategoryName = TradingServicePropertyCategoryName.GENERAL,
                    CategoryDisplayName = TradingServicePropertyCategoryName.GENERAL,
                    Name = TradingServicePropertyName.DECIMALS_BASE,
                    DisplayName = TradingServicePropertyName.DECIMALS_BASE,
                    Browsable = true,
                    Type = Quantum.Framework.GenericProperties.Enum.GenericPropertyType.Integer,
                    MinimumValue = 0,
                    MaximumValue = 10,
                    DefaultValue = 0,
                }
            };
        }

        #region Utility Functions

        public async Task<decimal> GetLastSoldPrice(string symbol = null)
        {
            var lastSoldOrder = await GetLastSoldOrder();
            return lastSoldOrder != null ? lastSoldOrder.Price : 0;
        }

        public async Task<Order> GetLastSoldOrder(string symbol = null)
        {
            if (string.IsNullOrEmpty(symbol))
                symbol = Symbol;

            var orders = await Api.GetOrdersAsync(User, symbol, -1, 15);
            for (int i = orders.Count() - 1; i >= 0; i--)
            {
                var order = orders.ElementAt(i);
                if (order.Side == OrderSide.Sell && order.Status == OrderStatus.Filled)
                    return order;
            }

            return null;
        }

        public async Task<decimal> GetLastBoughtPrice(string symbol = null)
        {
            var lastBoughtOrder = await GetLastBoughtOrder(symbol);
            return lastBoughtOrder != null ? lastBoughtOrder.Price : 0;
        }

        public async Task<Order> GetLastBoughtOrder(string symbol = null)
        {
            if (string.IsNullOrEmpty(symbol))
                symbol = Symbol;

            var orders = await Api.GetOrdersAsync(User, symbol, -1, 15);
            for (int i = orders.Count() - 1; i >= 0; i--)
            {
                var order = orders.ElementAt(i);
                if (order.Side == OrderSide.Buy && order.Status == OrderStatus.Filled)
                    return order;
                else if (order.Side == OrderSide.Buy && order.Status == OrderStatus.Canceled && (order.ExecutedQuantity / order.OriginalQuantity) >= 0.5m)
                    return order;
            }

            return null;
        }

        public async Task<decimal> GetBTCBalance()
        {
            var account = await Api.GetAccountInfoAsync(User);
            foreach (var balance in account.Balances)
            {
                if (balance.Free > 0 || balance.Locked > 0)
                {
                    if (balance.Asset == Asset.BTC.Symbol)
                        return balance.Free;
                }
            }

            return 0;
        }

        public async Task<decimal> GetUSDTBalance()
        {
            var account = await Api.GetAccountInfoAsync(User);
            foreach (var balance in account.Balances)
            {
                if (balance.Free > 0 || balance.Locked > 0)
                {
                    if (balance.Asset == Asset.USDT.Symbol)
                        return balance.Free;
                }
            }

            return 0;
        }

        public async Task<decimal> GetSymbolBalance(string symbol)
        {
            var account = await Api.GetAccountInfoAsync(User);
            foreach (var balance in account.Balances)
            {
                if (balance.Free > 0 || balance.Locked > 0)
                {
                    if (balance.Asset == symbol)
                        return balance.Free;
                }
            }

            return 0;
        }

        public async Task<decimal> GetBuyableAmount(decimal amountToBuy, decimal price)
        {
            var balanceSufficient = false;
            while (!balanceSufficient)
            {
                try
                {
                    await Api.TestPlaceAsync(new LimitOrder(User)
                    {
                        Symbol = Symbol,
                        Side = OrderSide.Buy,
                        Quantity = amountToBuy,
                        Price = price
                    });
                    balanceSufficient = true;
                }
                catch (Exception ex)
                {
                    amountToBuy = amountToBuy - (decimal)(1 / Math.Pow(10, Decimals));
                }
            }

            return amountToBuy;
        }


        #endregion

    }
}
