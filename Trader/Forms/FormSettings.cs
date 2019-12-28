using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Trader.TradingService.Enum;
using Trader.TradingService.Service;
using Trader.Utility;
using Trader.Utility.SettingsManager.Enum;
using Trader.Utility.SettingsManager.Manager;

namespace Trader.Forms
{
    public partial class FormSettings : Form
    {
        private string tradingSymbol;
        private List<string> tradingPool;

        private BindingSource tradingPoolBindingSource;

        public FormSettings()
        {
            InitializeComponent();
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            var apiKey = SettingsManager.Instance.Get(SettingName.API_KEY, string.Empty);
            var secretKey = SettingsManager.Instance.Get(SettingName.SECRET_KEY, string.Empty);

            // general-trading settings
            if (!string.IsNullOrEmpty(apiKey))
                textBoxApiKey.Text = EncryptionHelper.DecryptText(apiKey);
            if (!string.IsNullOrEmpty(secretKey))
                textBoxSecretKey.Text = EncryptionHelper.DecryptText(secretKey);

            tradingSymbol = SettingsManager.Instance.Get(SettingName.TRADING_SYMBOL, Defaults.TRADING_SYMBOL);
            labelSymbol.Text = tradingSymbol;

            numericUpDownDecimals.Value = SettingsManager.Instance.Get(SettingName.DECIMALS, Defaults.DECIMALS);

            var tradingType = SettingsManager.Instance.Get(SettingName.TRADING_TYPE, Defaults.TRADING_TYPE);
            if (tradingType == ThresholdTradingService.NAME)
                radioButtonThresholdTrading.Checked = true;
            else if (tradingType == ChartFollowTradingService.NAME)
                radioButtonChartFollowTrading.Checked = true;
            else if (tradingType == HitNRunTradingService.NAME)
                radioButtonHitNRunTrading.Checked = true;

            var unitTypeStr = SettingsManager.Instance.Get(SettingName.UNIT_TYPE, UnitType.Value.ToString());
            var unitType = (UnitType)System.Enum.Parse(typeof(UnitType), unitTypeStr, true);
            radioButtonUnitTypePercentage.Checked = unitType == UnitType.Percentage;
            radioButtonUnitTypeValue.Checked = unitType == UnitType.Value;

            var orderTypeStr = SettingsManager.Instance.Get(SettingName.ORDER_TYPE, OrderType.Limit.ToString());
            var orderType = (OrderType)System.Enum.Parse(typeof(OrderType), orderTypeStr, true);
            radioButtonOrderTypeLimit.Checked = orderType == OrderType.Limit;
            radioButtonOrderTypeMarket.Checked = orderType == OrderType.Market;

            // ThresholdTrading settings
            numericUpDownThresholdTradingBuyDifferenceThreshold.Value = SettingsManager.Instance.Get(ThresholdTradingPropertyName.BUY_DIFFERENCE_THRESHOLD, ThresholdTradingDefaults.BUY_DIFFERENCE_THRESHOLD);
            numericUpDownThresholdTradingSellDifferenceThreshold.Value = SettingsManager.Instance.Get(ThresholdTradingPropertyName.SELL_DIFFERENCE_THRESHOLD, ThresholdTradingDefaults.SELL_DIFFERENCE_THRESHOLD);
            numericUpDownThresholdTradingCheckInterval.Value = SettingsManager.Instance.Get(ThresholdTradingPropertyName.TRADING_CHECK_INTERVAL, ThresholdTradingDefaults.TRADING_CHECK_INTERVAL);
            numericUpDownThresholdTradingCheckTimeoutForBuying.Value = SettingsManager.Instance.Get(ThresholdTradingPropertyName.CHECK_TIMEOUT_FOR_BUYING, ThresholdTradingDefaults.CHECK_TIMEOUT_FOR_BUYING);

            tradingPool = SettingsManager.Instance.Get(SettingName.TRADING_POOL, new List<string>());

            // ChartFollowTrading Settings
            numericUpDownChartFollowTradingCheckInterval.Value = SettingsManager.Instance.Get(ChartFollowTradingPropertyName.TRADING_CHECK_INTERVAL, ChartFollowTradingDefaults.TRADING_CHECK_INTERVAL);
            numericUpDownChartFollowTradingIncraseIndicatorThreshold.Value = SettingsManager.Instance.Get(ChartFollowTradingPropertyName.INCREASE_INDICATOR_THRESHOLD, ChartFollowTradingDefaults.INCREASE_INDICATOR_THRESHOLD);
            numericUpDownChartFollowTradingDecreaseIndicatorThreshold.Value = SettingsManager.Instance.Get(ChartFollowTradingPropertyName.DECREASE_INDICATOR_THRESHOLD, ChartFollowTradingDefaults.DECREASE_INDICATOR_THRESHOLD);
            numericUpDownChartFollowTradingMinBuyDifferenceThreshold.Value = SettingsManager.Instance.Get(ChartFollowTradingPropertyName.MIN_BUY_DIFFERENCE_THRESHOLD, ChartFollowTradingDefaults.MIN_BUY_DIFFERENCE_THRESHOLD);
            numericUpDownChartFollowTradingMinSellDifferenceThreshold.Value = SettingsManager.Instance.Get(ChartFollowTradingPropertyName.MIN_SELL_DIFFERENCE_THRESHOLD, ChartFollowTradingDefaults.MIN_SELL_DIFFERENCE_THRESHOLD);
            numericUpDownChartFollowTradingStopLossThreshold.Value = SettingsManager.Instance.Get(ChartFollowTradingPropertyName.STOP_LOSS_THRESHOLD, 0m);
            numericUpDownChartFollowTradingLossBuyThreshold.Value = SettingsManager.Instance.Get(ChartFollowTradingPropertyName.LOSS_BUY_PERCENTAGE, 0m);
            numericUpDownChartFollowTradingStopLossAfterMinutes.Value = SettingsManager.Instance.Get(ChartFollowTradingPropertyName.STOP_LOSS_AFTER_MINUTES, 0m);
            numericUpDownChartFollowTradingLossBuyDays.Value = SettingsManager.Instance.Get(ChartFollowTradingPropertyName.LOSS_BUY_DAYS, 0m);

            // Hit&Run Trading Settings
            numericUpDownHitNRunTradingStablenessCheckInterval.Value = SettingsManager.Instance.Get(HitNRunTradingPropertyName.STABLENESS_CHECK_INVERTVAL, HitNRunTradingDefaults.STABLENESS_CHECK_INTERVAL);
            numericUpDownHitNRunTradingStablenessIndicatorDifference.Value = SettingsManager.Instance.Get(HitNRunTradingPropertyName.STABLENESS_INDICATOR_THRESHOLD, HitNRunTradingDefaults.STABLENESS_INDICATOR_THRESHOD);
            numericUpDownHitNRunTradingSellDifferenceThreshold.Value = SettingsManager.Instance.Get(HitNRunTradingPropertyName.SELL_DIFFERENCE_THRESHOLD, HitNRunTradingDefaults.SELL_DIFFERENCE_THRESHOLD);

            tradingPoolBindingSource = new BindingSource();
            tradingPoolBindingSource.DataSource = tradingPool;

            listBoxTradingPool.DataSource = tradingPoolBindingSource;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SettingsManager.Instance.Set(SettingName.API_KEY, EncryptionHelper.EncryptText(textBoxApiKey.Text));
            SettingsManager.Instance.Set(SettingName.SECRET_KEY, EncryptionHelper.EncryptText(textBoxSecretKey.Text));
            SettingsManager.Instance.Set(SettingName.TRADING_SYMBOL, tradingSymbol);
            SettingsManager.Instance.Set(SettingName.DECIMALS, (int)numericUpDownDecimals.Value);
            SettingsManager.Instance.Set(SettingName.TRADING_POOL, tradingPool);

            if (radioButtonChartFollowTrading.Checked)
                SettingsManager.Instance.Set(SettingName.TRADING_TYPE, ChartFollowTradingService.NAME);
            else if (radioButtonThresholdTrading.Checked)
                SettingsManager.Instance.Set(SettingName.TRADING_TYPE, ThresholdTradingService.NAME);
            else if (radioButtonHitNRunTrading.Checked)
                SettingsManager.Instance.Set(SettingName.TRADING_TYPE, HitNRunTradingService.NAME);

            if (radioButtonUnitTypeValue.Checked)
                SettingsManager.Instance.Set(SettingName.UNIT_TYPE, UnitType.Value.ToString());
            else if (radioButtonUnitTypePercentage.Checked)
                SettingsManager.Instance.Set(SettingName.UNIT_TYPE, UnitType.Percentage.ToString());

            if (radioButtonOrderTypeLimit.Checked)
                SettingsManager.Instance.Set(SettingName.ORDER_TYPE, OrderType.Limit.ToString());
            else if (radioButtonOrderTypeMarket.Checked)
                SettingsManager.Instance.Set(SettingName.ORDER_TYPE, OrderType.Market.ToString());

            // saving ThresholdTrading Settings
            SettingsManager.Instance.Set(ThresholdTradingPropertyName.BUY_DIFFERENCE_THRESHOLD, numericUpDownThresholdTradingBuyDifferenceThreshold.Value);
            SettingsManager.Instance.Set(ThresholdTradingPropertyName.SELL_DIFFERENCE_THRESHOLD, numericUpDownThresholdTradingSellDifferenceThreshold.Value);
            SettingsManager.Instance.Set(ThresholdTradingPropertyName.TRADING_CHECK_INTERVAL, (int)numericUpDownThresholdTradingCheckInterval.Value);
            SettingsManager.Instance.Set(ThresholdTradingPropertyName.CHECK_TIMEOUT_FOR_BUYING, (int)numericUpDownThresholdTradingCheckTimeoutForBuying.Value);

            // saving ChartFollowTrading Settings
            SettingsManager.Instance.Set(ChartFollowTradingPropertyName.TRADING_CHECK_INTERVAL, (int)numericUpDownChartFollowTradingCheckInterval.Value);
            SettingsManager.Instance.Set(ChartFollowTradingPropertyName.INCREASE_INDICATOR_THRESHOLD, numericUpDownChartFollowTradingIncraseIndicatorThreshold.Value);
            SettingsManager.Instance.Set(ChartFollowTradingPropertyName.DECREASE_INDICATOR_THRESHOLD, numericUpDownChartFollowTradingDecreaseIndicatorThreshold.Value);
            SettingsManager.Instance.Set(ChartFollowTradingPropertyName.MIN_BUY_DIFFERENCE_THRESHOLD, numericUpDownChartFollowTradingMinBuyDifferenceThreshold.Value);
            SettingsManager.Instance.Set(ChartFollowTradingPropertyName.MIN_SELL_DIFFERENCE_THRESHOLD, numericUpDownChartFollowTradingMinSellDifferenceThreshold.Value);

            SettingsManager.Instance.Set(ChartFollowTradingPropertyName.STOP_LOSS_THRESHOLD, numericUpDownChartFollowTradingStopLossThreshold.Value);
            SettingsManager.Instance.Set(ChartFollowTradingPropertyName.STOP_LOSS_AFTER_MINUTES, numericUpDownChartFollowTradingStopLossAfterMinutes.Value);
            SettingsManager.Instance.Set(ChartFollowTradingPropertyName.LOSS_BUY_PERCENTAGE, numericUpDownChartFollowTradingLossBuyThreshold.Value);
            SettingsManager.Instance.Set(ChartFollowTradingPropertyName.LOSS_BUY_DAYS, numericUpDownChartFollowTradingLossBuyDays.Value);

            // saving hitNRunTrading Settings
            SettingsManager.Instance.Set(HitNRunTradingPropertyName.STABLENESS_CHECK_INVERTVAL, (int)numericUpDownHitNRunTradingStablenessCheckInterval.Value);
            SettingsManager.Instance.Set(HitNRunTradingPropertyName.STABLENESS_INDICATOR_THRESHOLD, (int)numericUpDownHitNRunTradingStablenessIndicatorDifference.Value);
            SettingsManager.Instance.Set(HitNRunTradingPropertyName.SELL_DIFFERENCE_THRESHOLD, numericUpDownHitNRunTradingSellDifferenceThreshold.Value);

            DialogResult = DialogResult.OK;
        }

        private void buttonEditSymbol_Click(object sender, EventArgs e)
        {
            var formSymbolList = new FormSymbolList();
            if (formSymbolList.ShowDialog(this) == DialogResult.OK)
            {
                tradingSymbol = formSymbolList.Symbol;
                labelSymbol.Text = tradingSymbol;
            }
        }
    }
}
