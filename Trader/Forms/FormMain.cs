using Binance;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Trader.Data;
using Trader.Enum;
using Trader.TradingService.Service;
using Trader.Utility;
using Trader.Utility.LogManager.Manager;
using Trader.Utility.SettingsManager.Enum;
using Trader.Utility.SettingsManager.Manager;

namespace Trader.Forms
{
    public partial class FormMain : Form
    {
        #region Constants

        private const string WALLET_FORMAT = "{0} BTC / ${1} / {2} TL";
        private const int WALLET_REFRESH_INTERVAL = 300; // in seconds
        private const int LAST_NUMBER_OF_SYMBOL_LOG = 3600; // almost in seconds

        #endregion

        #region Private Variables

        private Timer timerWalletRefresh;
        private static State state = State.Ready;
        private TradingService.Service.TradingService tradingService;

        private bool firstBalanceShown = false;

        #endregion

        public FormMain()
        {
            InitializeComponent();
        }

        #region Common Events - Clicks

        private void FormMain_Load(object sender, EventArgs e)
        {
            SetupTradingService();
            SetupTimerWalletRefresh();

            LogManager.Instance.OnTradeLogged += LogManager_OnTradeLogged;
            LogManager.Instance.OnSymbolLogged += LogManager_OnSymbolLogged;

            FileLogger.Instance.OnFileLogged += Instance_OnFileLogged;

            RefreshButtons();
            RefreshDataOnUI();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (tradingService.Active)
            {
                var dialogResult = MessageBox.Show(this, "Are you sure about to exit? Trading will be stopped!", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    tradingService.StopTrading();
                    tradingService.Deinitialize();
                    LogManager.Instance.SaveLogs();
                }
                else
                    e.Cancel = true;
            }
            else
                LogManager.Instance.SaveLogs();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var formSettings = new FormSettings();
            if (formSettings.ShowDialog(this) == DialogResult.OK)
            {
                RefreshButtons();

                if (tradingService.Active)
                {
                    var dialogResult = MessageBox.Show(this, "Do you want to reset trading service? If you don't, your trading settings will not be used.", "Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        tradingService.StopTrading();
                        System.Threading.Thread.Sleep(500);
                        SetupTradingService();
                        RefreshButtons();
                    }
                }
                else
                    SetupTradingService();
            }
        }

        private void buttonStartTrade_Click(object sender, EventArgs e)
        {
            tradingService.StartTrading();

            RefreshStateLabel();
            RefreshButtons();
        }

        private void buttonStopTrade_Click(object sender, EventArgs e)
        {
            tradingService.StopTrading();

            RefreshStateLabel();
            RefreshButtons();
        }

        #endregion

        #region Service Events

        private void LogManager_OnSymbolLogged(object sender, Utility.LogManager.Events.SymbolLogEventArgs e)
        {
            if (checkBoxSymbolLogActive.Checked)
            {
                var symbolLogs = LogManager.Instance.GetSymbolLogs(LAST_NUMBER_OF_SYMBOL_LOG);
                symbolLogs.Reverse();

                Invoke(new Action(() =>
                {
                    if (!Disposing && !IsDisposed)
                    {
                        dataGridViewSymbol.DataSource = symbolLogs;
                        dataGridViewSymbol.Columns["Date"].DefaultCellStyle.Format = "MM.dd.yyyy HH:mm:ss";

                        RefreshChange();
                    }
                }));
            }
        }

        private void LogManager_OnTradeLogged(object sender, EventArgs e)
        {
            var tradeLogs = LogManager.Instance.GetTradeLogs();
            tradeLogs.Reverse();

            Invoke(new Action(() =>
            {
                dataGridViewTrades.DataSource = tradeLogs;
                dataGridViewTrades.Columns["Date"].DefaultCellStyle.Format = "MM.dd.yyyy HH:mm:ss";
            }));
        }

        private void WalletRefreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshWallet();
        }

        private void Instance_OnFileLogged(object sender, Utility.Events.FileLoggedEventArgs e)
        {
            if (checkBoxTradingServiceLogActive.Checked)
            {
                richTextBox.Text += e.Text + "\n";

                richTextBox.SelectionStart = richTextBox.Text.Length;
                richTextBox.ScrollToCaret();
            }
        }

        #endregion

        #region Initialize Operations

        private void SetupTimerWalletRefresh()
        {
            timerWalletRefresh = new Timer();
            timerWalletRefresh.Interval = 1000 * WALLET_REFRESH_INTERVAL;
            timerWalletRefresh.Tick += WalletRefreshTimer_Tick;
            timerWalletRefresh.Start();

            RefreshWallet();
        }

        private void SetupTradingService()
        {
            if (tradingService != null)
                tradingService.Deinitialize();

            tradingService = GetTradingService();
            tradingService.Initialize();
        }

        private TradingService.Service.TradingService GetTradingService()
        {
            var tradingServiceName = SettingsManager.Instance.Get(SettingName.TRADING_TYPE, Defaults.TRADING_TYPE);
            switch (tradingServiceName)
            {
                case ChartFollowTradingService.NAME:
                    return new ChartFollowTradingService();
                case ThresholdTradingService.NAME:
                    return new ThresholdTradingService();
                case HitNRunTradingService.NAME:
                    return new HitNRunTradingService();
                default:
                    return null;
            }
        }

        #endregion

        #region UI Operations

        private void RefreshButtons()
        {
            buttonStartTrading.Enabled = !tradingService.Active;
            buttonStopTrade.Enabled = tradingService.Active;
        }

        private void RefreshStateLabel()
        {
            /*
            switch (state)
            {
                case State.Ready:
                    toolStripStatusLabel.Text = "Ready";
                    break;
                case State.Processing:
                    toolStripStatusLabel.Text = "Processing";
                    break;
                case State.WaitingForBuy:
                    toolStripStatusLabel.Text = "Waiting for to Buy";
                    break;
                case State.WaitingForSell:
                    toolStripStatusLabel.Text = "Waiting for to Sell";
                    break;
                case State.WaitingForOpenBuyOrder:
                    toolStripStatusLabel.Text = "Waiting for Open Buy Order";
                    break;
                case State.WaitingForOpenSellOrder:
                    toolStripStatusLabel.Text = "Waiting for Open Sell Order";
                    break;
                default:
                    break;
            }
            */
        }

        private async void RefreshWallet()
        {
            try
            {
                var account = await tradingService.Api.GetAccountInfoAsync(tradingService.User);
                var prices = await tradingService.Api.GetPricesAsync();
                decimal totalBTC = 0;
                foreach (var balance in account.Balances)
                {
                    if (balance.Free > 0 || balance.Locked > 0)
                    {
                        if (balance.Asset == Asset.BTC.Symbol)
                            totalBTC += (balance.Free + balance.Locked);
                        else
                        {
                            if (balance.Asset == "USDT")
                            {
                                var priceBTC = prices.Where(x => x.Symbol == "BTCUSDT").FirstOrDefault().Value;
                                totalBTC += (balance.Free + balance.Locked) / priceBTC;
                            }
                            else
                            {
                                var symbol = TradingService.Utility.TradingServiceHelper.GetSymbolByName(balance.Asset + "BTC");
                                if (symbol == null)
                                {
                                    //throw new Exception(balance.Asset + "BTC asset is null.");
                                }
                                else
                                {
                                    var price = prices.Where(x => x.Symbol == symbol).FirstOrDefault().Value;
                                    totalBTC += price * (balance.Free + balance.Locked);
                                }
                            }
                        }
                    }
                }

                var btcUsdPrice = await tradingService.Api.GetPriceAsync(Symbol.BTC_USDT);

                var btcStringFormat = string.Format("{0:0.########}", totalBTC);
                var usdStringFormat = string.Format("{0:0}", totalBTC * btcUsdPrice.Value);
                var tlStringFormat = string.Format("{0:0}", totalBTC * btcUsdPrice.Value * (decimal)ServiceHelper.USDtoTRY());

                toolStripStatusLabelFunds.Text = string.Format(WALLET_FORMAT, btcStringFormat, usdStringFormat, tlStringFormat);

                if (!firstBalanceShown)
                {
                    firstBalanceShown = true;
                    toolStripStatusLabel.Text = string.Format("First Balance: {0} BTC / ${1}", btcStringFormat, usdStringFormat);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void RefreshDataOnUI()
        {
            comboBoxLastChange.DataSource = LastTimeSpan.GetLastTimeSpans();
            comboBoxLastChange.SelectedIndex = 0;

            var tradeLogs = LogManager.Instance.GetTradeLogs();
            tradeLogs.Reverse();

            dataGridViewTrades.DataSource = tradeLogs;
            dataGridViewTrades.Columns["Date"].DefaultCellStyle.Format = "MM.dd.yyyy HH:mm:ss";
        }

        private void RefreshChange()
        {
            var lastTimeSpan = (LastTimeSpan)comboBoxLastChange.SelectedItem;

            decimal changePrice = LogManager.Instance.GetChangePriceByMinutes(lastTimeSpan.Minutes);
            decimal changePercent = LogManager.Instance.GetChangePercentByMinutes(lastTimeSpan.Minutes);

            var formatPrice = "0.########";
            var formatPercent = "0.###";

            if (changePrice > 0)
                labelChange.ForeColor = Color.DarkGreen;
            else if (changePrice < 0)
                labelChange.ForeColor = Color.DarkRed;
            else
                labelChange.ForeColor = Color.Black;

            labelChange.Text = string.Format("{0}    {1}%", changePrice.ToString(formatPrice), changePercent.ToString(formatPercent));
        }

        #endregion

        #region UI Control Events

        private void comboBoxLastChange_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshChange();
        }

        private void dataGridViewTrades_DataSourceChanged(object sender, EventArgs e)
        {
            if (dataGridViewTrades.Columns.Count > 0)
            {
                dataGridViewTrades.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridViewTrades.Columns[0].Width = 32;
            }

            foreach (DataGridViewRow row in dataGridViewTrades.Rows)
            {
                if (Convert.ToString(row.Cells[0].Value) == "Buy")
                    row.DefaultCellStyle.BackColor = Defaults.Green;
                else
                    row.DefaultCellStyle.BackColor = Defaults.Red;
            }
        }

        private void buttonClearTradingServiceLog_Click(object sender, EventArgs e)
        {
            richTextBox.Text = string.Empty;
        }

        #endregion
    }
}
