namespace Trader.Forms
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelFunds = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.applicationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonStopTrade = new System.Windows.Forms.Button();
            this.buttonStartTrading = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageTradeLog = new System.Windows.Forms.TabPage();
            this.dataGridViewTrades = new System.Windows.Forms.DataGridView();
            this.tabPageTradingServiceLog = new System.Windows.Forms.TabPage();
            this.buttonClearTradingServiceLog = new System.Windows.Forms.Button();
            this.checkBoxTradingServiceLogActive = new System.Windows.Forms.CheckBox();
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.tabPageLog = new System.Windows.Forms.TabPage();
            this.labelChange = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxLastChange = new System.Windows.Forms.ComboBox();
            this.dataGridViewSymbol = new System.Windows.Forms.DataGridView();
            this.checkBoxSymbolLogActive = new System.Windows.Forms.CheckBox();
            this.statusStrip.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageTradeLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTrades)).BeginInit();
            this.tabPageTradingServiceLog.SuspendLayout();
            this.tabPageLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSymbol)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.toolStripStatusLabelFunds});
            this.statusStrip.Location = new System.Drawing.Point(0, 512);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(499, 22);
            this.statusStrip.TabIndex = 0;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(39, 17);
            this.toolStripStatusLabel.Text = "Ready";
            // 
            // toolStripStatusLabelFunds
            // 
            this.toolStripStatusLabelFunds.Name = "toolStripStatusLabelFunds";
            this.toolStripStatusLabelFunds.Size = new System.Drawing.Size(445, 17);
            this.toolStripStatusLabelFunds.Spring = true;
            this.toolStripStatusLabelFunds.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.applicationToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(499, 24);
            this.menuStrip.TabIndex = 1;
            this.menuStrip.Text = "menuStrip1";
            // 
            // applicationToolStripMenuItem
            // 
            this.applicationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem});
            this.applicationToolStripMenuItem.Name = "applicationToolStripMenuItem";
            this.applicationToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.applicationToolStripMenuItem.Text = "Application";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // buttonStopTrade
            // 
            this.buttonStopTrade.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStopTrade.Location = new System.Drawing.Point(384, 482);
            this.buttonStopTrade.Name = "buttonStopTrade";
            this.buttonStopTrade.Size = new System.Drawing.Size(103, 23);
            this.buttonStopTrade.TabIndex = 3;
            this.buttonStopTrade.Text = "Stop Trading";
            this.buttonStopTrade.UseVisualStyleBackColor = true;
            this.buttonStopTrade.Click += new System.EventHandler(this.buttonStopTrade_Click);
            // 
            // buttonStartTrading
            // 
            this.buttonStartTrading.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStartTrading.Location = new System.Drawing.Point(272, 482);
            this.buttonStartTrading.Name = "buttonStartTrading";
            this.buttonStartTrading.Size = new System.Drawing.Size(103, 23);
            this.buttonStartTrading.TabIndex = 4;
            this.buttonStartTrading.Text = "Start Trading";
            this.buttonStartTrading.UseVisualStyleBackColor = true;
            this.buttonStartTrading.Click += new System.EventHandler(this.buttonStartTrade_Click);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabPageTradeLog);
            this.tabControl.Controls.Add(this.tabPageTradingServiceLog);
            this.tabControl.Controls.Add(this.tabPageLog);
            this.tabControl.Location = new System.Drawing.Point(12, 27);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(475, 449);
            this.tabControl.TabIndex = 5;
            // 
            // tabPageTradeLog
            // 
            this.tabPageTradeLog.Controls.Add(this.dataGridViewTrades);
            this.tabPageTradeLog.Location = new System.Drawing.Point(4, 22);
            this.tabPageTradeLog.Name = "tabPageTradeLog";
            this.tabPageTradeLog.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTradeLog.Size = new System.Drawing.Size(467, 423);
            this.tabPageTradeLog.TabIndex = 0;
            this.tabPageTradeLog.Text = "Trade Log";
            this.tabPageTradeLog.UseVisualStyleBackColor = true;
            // 
            // dataGridViewTrades
            // 
            this.dataGridViewTrades.AllowUserToAddRows = false;
            this.dataGridViewTrades.AllowUserToDeleteRows = false;
            this.dataGridViewTrades.AllowUserToResizeRows = false;
            this.dataGridViewTrades.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewTrades.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridViewTrades.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewTrades.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewTrades.Name = "dataGridViewTrades";
            this.dataGridViewTrades.ReadOnly = true;
            this.dataGridViewTrades.RowHeadersVisible = false;
            this.dataGridViewTrades.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewTrades.Size = new System.Drawing.Size(461, 417);
            this.dataGridViewTrades.TabIndex = 1;
            this.dataGridViewTrades.DataSourceChanged += new System.EventHandler(this.dataGridViewTrades_DataSourceChanged);
            // 
            // tabPageTradingServiceLog
            // 
            this.tabPageTradingServiceLog.Controls.Add(this.buttonClearTradingServiceLog);
            this.tabPageTradingServiceLog.Controls.Add(this.checkBoxTradingServiceLogActive);
            this.tabPageTradingServiceLog.Controls.Add(this.richTextBox);
            this.tabPageTradingServiceLog.Location = new System.Drawing.Point(4, 22);
            this.tabPageTradingServiceLog.Name = "tabPageTradingServiceLog";
            this.tabPageTradingServiceLog.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTradingServiceLog.Size = new System.Drawing.Size(467, 423);
            this.tabPageTradingServiceLog.TabIndex = 2;
            this.tabPageTradingServiceLog.Text = "Trading Service Log";
            this.tabPageTradingServiceLog.UseVisualStyleBackColor = true;
            // 
            // buttonClearTradingServiceLog
            // 
            this.buttonClearTradingServiceLog.Location = new System.Drawing.Point(6, 22);
            this.buttonClearTradingServiceLog.Name = "buttonClearTradingServiceLog";
            this.buttonClearTradingServiceLog.Size = new System.Drawing.Size(86, 23);
            this.buttonClearTradingServiceLog.TabIndex = 5;
            this.buttonClearTradingServiceLog.Text = "Clear";
            this.buttonClearTradingServiceLog.UseVisualStyleBackColor = true;
            this.buttonClearTradingServiceLog.Click += new System.EventHandler(this.buttonClearTradingServiceLog_Click);
            // 
            // checkBoxTradingServiceLogActive
            // 
            this.checkBoxTradingServiceLogActive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxTradingServiceLogActive.AutoSize = true;
            this.checkBoxTradingServiceLogActive.Checked = true;
            this.checkBoxTradingServiceLogActive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTradingServiceLogActive.Location = new System.Drawing.Point(405, 26);
            this.checkBoxTradingServiceLogActive.Name = "checkBoxTradingServiceLogActive";
            this.checkBoxTradingServiceLogActive.Size = new System.Drawing.Size(56, 17);
            this.checkBoxTradingServiceLogActive.TabIndex = 1;
            this.checkBoxTradingServiceLogActive.Text = "Active";
            this.checkBoxTradingServiceLogActive.UseVisualStyleBackColor = true;
            // 
            // richTextBox
            // 
            this.richTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox.Location = new System.Drawing.Point(3, 49);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.ReadOnly = true;
            this.richTextBox.Size = new System.Drawing.Size(461, 369);
            this.richTextBox.TabIndex = 0;
            this.richTextBox.Text = "";
            // 
            // tabPageLog
            // 
            this.tabPageLog.Controls.Add(this.labelChange);
            this.tabPageLog.Controls.Add(this.label1);
            this.tabPageLog.Controls.Add(this.comboBoxLastChange);
            this.tabPageLog.Controls.Add(this.dataGridViewSymbol);
            this.tabPageLog.Controls.Add(this.checkBoxSymbolLogActive);
            this.tabPageLog.Location = new System.Drawing.Point(4, 22);
            this.tabPageLog.Name = "tabPageLog";
            this.tabPageLog.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLog.Size = new System.Drawing.Size(467, 423);
            this.tabPageLog.TabIndex = 1;
            this.tabPageLog.Text = "Symbol Log";
            this.tabPageLog.UseVisualStyleBackColor = true;
            // 
            // labelChange
            // 
            this.labelChange.AutoSize = true;
            this.labelChange.Location = new System.Drawing.Point(175, 17);
            this.labelChange.Name = "labelChange";
            this.labelChange.Size = new System.Drawing.Size(0, 13);
            this.labelChange.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(125, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Change:";
            // 
            // comboBoxLastChange
            // 
            this.comboBoxLastChange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLastChange.FormattingEnabled = true;
            this.comboBoxLastChange.Location = new System.Drawing.Point(6, 14);
            this.comboBoxLastChange.Name = "comboBoxLastChange";
            this.comboBoxLastChange.Size = new System.Drawing.Size(113, 21);
            this.comboBoxLastChange.TabIndex = 2;
            this.comboBoxLastChange.SelectedIndexChanged += new System.EventHandler(this.comboBoxLastChange_SelectedIndexChanged);
            // 
            // dataGridViewSymbol
            // 
            this.dataGridViewSymbol.AllowUserToAddRows = false;
            this.dataGridViewSymbol.AllowUserToDeleteRows = false;
            this.dataGridViewSymbol.AllowUserToResizeRows = false;
            this.dataGridViewSymbol.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewSymbol.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewSymbol.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewSymbol.Location = new System.Drawing.Point(3, 49);
            this.dataGridViewSymbol.Name = "dataGridViewSymbol";
            this.dataGridViewSymbol.ReadOnly = true;
            this.dataGridViewSymbol.RowHeadersVisible = false;
            this.dataGridViewSymbol.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewSymbol.Size = new System.Drawing.Size(461, 371);
            this.dataGridViewSymbol.TabIndex = 1;
            // 
            // checkBoxSymbolLogActive
            // 
            this.checkBoxSymbolLogActive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxSymbolLogActive.AutoSize = true;
            this.checkBoxSymbolLogActive.Checked = true;
            this.checkBoxSymbolLogActive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSymbolLogActive.Location = new System.Drawing.Point(405, 26);
            this.checkBoxSymbolLogActive.Name = "checkBoxSymbolLogActive";
            this.checkBoxSymbolLogActive.Size = new System.Drawing.Size(56, 17);
            this.checkBoxSymbolLogActive.TabIndex = 0;
            this.checkBoxSymbolLogActive.Text = "Active";
            this.checkBoxSymbolLogActive.UseVisualStyleBackColor = true;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(499, 534);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.buttonStartTrading);
            this.Controls.Add(this.buttonStopTrade);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Trader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabPageTradeLog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTrades)).EndInit();
            this.tabPageTradingServiceLog.ResumeLayout(false);
            this.tabPageTradingServiceLog.PerformLayout();
            this.tabPageLog.ResumeLayout(false);
            this.tabPageLog.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSymbol)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem applicationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelFunds;
        private System.Windows.Forms.Button buttonStopTrade;
        private System.Windows.Forms.Button buttonStartTrading;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageTradeLog;
        private System.Windows.Forms.DataGridView dataGridViewTrades;
        private System.Windows.Forms.TabPage tabPageLog;
        private System.Windows.Forms.DataGridView dataGridViewSymbol;
        private System.Windows.Forms.CheckBox checkBoxSymbolLogActive;
        private System.Windows.Forms.Label labelChange;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxLastChange;
        private System.Windows.Forms.TabPage tabPageTradingServiceLog;
        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.CheckBox checkBoxTradingServiceLogActive;
        private System.Windows.Forms.Button buttonClearTradingServiceLog;
    }
}

