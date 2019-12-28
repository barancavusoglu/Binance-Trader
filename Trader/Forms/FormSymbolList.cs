using Binance;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Trader.Forms
{
    public partial class FormSymbolList : Form
    {
        private List<string> symbolList;

        public string Symbol { get; set; }

        public FormSymbolList()
        {
            InitializeComponent();
        }

        private async void FormSymbolList_LoadAsync(object sender, EventArgs e)
        {
            var api = new BinanceApi();
            var symbols = await api.GetSymbolsAsync();
            symbolList = symbols/*.Where(x => x.QuoteAsset.Symbol == "BTC")*/.OrderBy(x => x.BaseAsset.Symbol).Select(x => x.ToString()).ToList();
            comboBoxSymbols.DataSource = symbols/*.Where(x => x.QuoteAsset.Symbol == "BTC")*/.OrderBy(x => x.BaseAsset.Symbol).Select(x => x.BaseAsset.Symbol + " - " + x.QuoteAsset.Symbol).ToList();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (comboBoxSymbols.SelectedIndex != -1)
            {
                Symbol = symbolList[comboBoxSymbols.SelectedIndex].ToString();
                DialogResult = DialogResult.OK;
            }
            else
                MessageBox.Show(this, "Please select a symbol.");
        }
    }
}
