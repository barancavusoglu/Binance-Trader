using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Trader.Utility.LogManager.Data;
using Trader.Utility.LogManager.Events;

namespace Trader.Utility.LogManager.Manager
{
    public class LogManager
    {
        public event EventHandler<TradeLogEventArgs> OnTradeLogged;
        public event EventHandler<SymbolLogEventArgs> OnSymbolLogged;

        #region Singleton Members
        private static LogManager instance;
        public static LogManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new LogManager();
                return instance;
            }
        }
        #endregion

        #region Constants

        private const int SAVE_INTERVAL = 15; // minutes
        private const int MAX_LOGS_NUMBER = 10000;

        #endregion

        #region Private Variables

        private bool isLocked = false;

        #endregion

        public LogManager()
        {
            symbolLogs = new List<SymbolLog>();
            tradeLogs = new List<TradeLog>();

            //LoadSymbolLogs();
            //LoadTradeLogs();

            savingTimer = new Timer()
            {
                Interval = 1000 * 60 * SAVE_INTERVAL
            };
            savingTimer.Tick += SavingTimer_Tick;
            //savingTimer.Start();
        }

        #region Private Variables

        private List<SymbolLog> symbolLogs;
        private List<TradeLog> tradeLogs;
        private Timer savingTimer;

        #endregion

        public void AddLog(Log log, bool dateTimeNow = true)
        {
            if (!isLocked)
            {
                if (dateTimeNow)
                    log.Date = DateTime.Now;

                if (log is SymbolLog symbolLog)
                {
                    if (symbolLogs.Count > MAX_LOGS_NUMBER)
                        symbolLogs.RemoveAt(0);

                    symbolLogs.Add(symbolLog);

                    OnSymbolLogged?.Invoke(this, new SymbolLogEventArgs()
                    {
                        SymbolLog = symbolLog
                    });
                }
                else if (log is TradeLog tradeLog)
                {
                    // check for existence
                    var exist = false;
                    if (tradeLog.OrderId != 0)
                        exist = tradeLogs.Any(x => x.OrderId == tradeLog.OrderId);

                    if (!exist)
                    {
                        if (tradeLogs.Count > MAX_LOGS_NUMBER)
                            tradeLogs.RemoveAt(0);

                        tradeLogs.Add(tradeLog);

                        tradeLogs = tradeLogs.OrderBy(x => x.Date).ToList();

                        OnTradeLogged?.Invoke(this, new TradeLogEventArgs()
                        {
                            TradeLog = tradeLog
                        });
                    }
                }
            }
        }

        public void ClearTradeLogs()
        {
            tradeLogs.Clear();
        }

        public TradeLog GetLastTradeLog(string symbol = null)
        {
            isLocked = true;

            TradeLog tradeLog = null;

            if (tradeLogs.Any())
            {
                if (string.IsNullOrEmpty(symbol))
                {
                    tradeLog = tradeLogs.Last();
                }
                else
                    tradeLog = tradeLogs.Where(a => a.Symbol == symbol).LastOrDefault();
            }

            isLocked = false;

            return tradeLog;
        }

        public SymbolLog GetLastSymbolLog(string symbol = null, int lastMs = 0)
        {
            isLocked = true;

            SymbolLog symbolLog = null;

            if (lastMs == 0)
            {
                try
                {
                    if (symbolLogs.Count > 0)
                    {
                        if (string.IsNullOrEmpty(symbol))
                        {
                            symbolLog = symbolLogs.Last();
                        }
                        else
                            symbolLog = symbolLogs.Where(a => a.Symbol == symbol).LastOrDefault();
                    }
                }
                catch (Exception ex)
                {
                    isLocked = false;
                    return null;
                }
            }
            else
            {
                try
                {
                    if (symbolLogs.Count > 0)
                    {
                        if (string.IsNullOrEmpty(symbol))
                        {
                            symbol = symbolLogs.Last().Symbol;
                        }

                        var logCount = lastMs / 1000;
                        var lastNLogs = symbolLogs.Skip(Math.Max(0, symbolLogs.Count() - logCount));

                        var total = 0m;
                        foreach (var log in lastNLogs)
                            total += log.Value;

                        var result = total / lastNLogs.Count();
                        result = result.RoundTo(6);

                        if (result > 0)
                            symbolLog = new SymbolLog()
                            {
                                Date = DateTime.Now,
                                Symbol = symbol,
                                Value = result
                            };
                    }
                }
                catch (Exception)
                {
                    isLocked = false;
                    return null;
                }
            }

            isLocked = false;

            return symbolLog;
        }

        private void SavingTimer_Tick(object sender, EventArgs e)
        {
            isLocked = true;

            if (tradeLogs.Any())
                SaveTradeLogs();

            if (symbolLogs.Any())
                SaveSymbolLogs();

            isLocked = false;
        }

        #region Save/Load Functions

        public void SaveLogs()
        {
            SaveTradeLogs();
            SaveSymbolLogs();
        }

        private void SaveTradeLogs()
        {
            var tradeLogsAfterStartDate = tradeLogs.Where(x => x.Date >= GlobalSettings.StartDate);
            if (tradeLogsAfterStartDate != null && tradeLogsAfterStartDate.Any())
            {
                if (!Directory.Exists(Path.GetDirectoryName(GlobalSettings.LOG_TRADE_FILENAME)))
                    Directory.CreateDirectory(Path.GetDirectoryName(GlobalSettings.LOG_TRADE_FILENAME));

                File.WriteAllText(GlobalSettings.LOG_TRADE_FILENAME, JsonConvert.SerializeObject(tradeLogs.Where(x => x.Date >= GlobalSettings.StartDate).ToList()));
            }
        }

        private void SaveSymbolLogs()
        {
            var symbolLogsAfterStartDate = symbolLogs.Where(x => x.Date >= GlobalSettings.StartDate);
            if (symbolLogsAfterStartDate != null && symbolLogsAfterStartDate.Any())
            {
                if (!Directory.Exists(Path.GetDirectoryName(GlobalSettings.LOG_SYMBOL_FILENAME)))
                    Directory.CreateDirectory(Path.GetDirectoryName(GlobalSettings.LOG_SYMBOL_FILENAME));

                File.WriteAllText(GlobalSettings.LOG_SYMBOL_FILENAME, JsonConvert.SerializeObject(symbolLogs.Where(x => x.Date >= GlobalSettings.StartDate).ToList()));
            }
        }

        public void LoadSymbolLogs()
        {
            if (Directory.Exists(GlobalSettings.LOG_SYMBOL_FILEPATH))
            {
                var directoryInfo = new DirectoryInfo(GlobalSettings.LOG_SYMBOL_FILEPATH);
                var files = directoryInfo.GetFiles(GlobalSettings.LOG_SYMBOL_FILENAME_PREFIX + "*.*");

                foreach (var file in files)
                {
                    var symbolLogsContent = File.ReadAllText(file.FullName);
                    var symbolLogsFromFile = JsonConvert.DeserializeObject<List<SymbolLog>>(symbolLogsContent);
                    symbolLogs.AddRange(symbolLogsFromFile);
                }
            }
        }

        public void LoadTradeLogs()
        {
            if (Directory.Exists(GlobalSettings.LOG_TRADE_FILEPATH))
            {
                var directoryInfo = new DirectoryInfo(GlobalSettings.LOG_TRADE_FILEPATH);
                var files = directoryInfo.GetFiles(GlobalSettings.LOG_TRADE_FILENAME_PREFIX + "*.*");

                foreach (var file in files)
                {
                    var tradeLogsContent = File.ReadAllText(file.FullName);
                    var tradeLogsFromFile = JsonConvert.DeserializeObject<List<TradeLog>>(tradeLogsContent);
                    tradeLogs.AddRange(tradeLogsFromFile);
                }
            }
        }

        #endregion

        #region Statistic Functions

        public List<TradeLog> GetTradeLogs(int limit = 0)
        {
            isLocked = true;

            var tradeLogs_ = new List<TradeLog>();
            if (limit == 0)
            {
                if (tradeLogs.Any())
                    tradeLogs_.AddRange(tradeLogs.GetRange(0, tradeLogs.Count));
            }
            else
            {
                if (tradeLogs.Any() && tradeLogs.Count > limit)
                    tradeLogs_.AddRange(tradeLogs.GetRange(tradeLogs.Count - limit, limit));
                else if (tradeLogs.Any())
                    tradeLogs_.AddRange(tradeLogs.GetRange(0, tradeLogs.Count));
            }

            isLocked = false;

            return tradeLogs_;
        }
        public List<SymbolLog> GetSymbolLogs(int limit = 0)
        {
            isLocked = true;

            var symbolLogs_ = new List<SymbolLog>();
            if (limit == 0)
            {
                if (symbolLogs.Any())
                    symbolLogs_.AddRange(symbolLogs.GetRange(0, symbolLogs.Count));
            }
            else
            {
                if (symbolLogs.Any() && symbolLogs.Count > limit)
                    symbolLogs_.AddRange(symbolLogs.GetRange(symbolLogs.Count - limit, limit));
                else if (symbolLogs.Any())
                    symbolLogs_.AddRange(symbolLogs.GetRange(0, symbolLogs.Count));
            }

            isLocked = false;

            return symbolLogs_;
        }

        public decimal GetChangePercentAfterDate(DateTime date)
        {
            return GetChangePercentByTimeSpan(date, DateTime.Now);
        }
        public decimal GetChangePriceAfterDate(DateTime date)
        {
            return GetChangePriceByTimeSpan(date, DateTime.Now);
        }

        public decimal GetChangePercentByTimeSpan(DateTime date1, DateTime date2)
        {
            if (date1 > date2)
                throw new Exception("Date 2 must be after date 1");

            var symbols_ = new List<SymbolLog>();
            for (int i = symbolLogs.Count - 1; i >= 0; i--)
            {
                if (symbolLogs[i].Date >= date1 && symbolLogs[i].Date <= date2)
                    symbols_.Add(symbolLogs[i]);
                else if (symbolLogs[i].Date < date1)
                    break;
            }

            if (!symbols_.Any())
                return (symbols_.First().Value - symbols_.Last().Value) / symbols_.Last().Value;
            else
                return 0;
        }
        public decimal GetChangePriceByTimeSpan(DateTime date1, DateTime date2)
        {
            if (date1 > date2)
                throw new Exception("Date 2 must be after date 1");

            var symbols_ = new List<SymbolLog>();
            for (int i = symbolLogs.Count - 1; i >= 0; i--)
            {
                if (symbolLogs[i].Date >= date1 && symbolLogs[i].Date <= date2)
                    symbols_.Add(symbolLogs[i]);
                else if (symbolLogs[i].Date < date1)
                    break;
            }

            if (symbols_.Any())
                return symbols_.First().Value - symbols_.Last().Value;
            else
                return 0;
        }

        public decimal GetChangePercentByMinutes(int minutes)
        {
            return GetChangePercentBySeconds(minutes * 60);
        }
        public decimal GetChangePriceByMinutes(int minutes)
        {
            return GetChangePriceBySeconds(minutes * 60);
        }

        public decimal GetChangePercentBySeconds(int seconds)
        {
            var symbols_ = GetSymbolLogsBySeconds(seconds);

            if (symbols_.Any())
                return (symbols_.First().Value - symbols_.Last().Value) / symbols_.Last().Value;
            else
                return 0;
        }
        public decimal GetChangePriceBySeconds(int seconds)
        {
            var symbols_ = GetSymbolLogsBySeconds(seconds);

            if (symbols_.Any())
                return symbols_.First().Value - symbols_.Last().Value;
            else
                return 0;
        }

        public decimal GetChangePriceByMilliseconds(int milliseconds)
        {
            var symbols_ = GetSymbolLogsByMilliSeconds(milliseconds);

            if (symbols_.Any())
                return symbols_.First().Value - symbols_.Last().Value;
            else
                return 0;
        }

        public List<SymbolLog> GetSymbolLogsByMinutes(int minutes)
        {
            return GetSymbolLogsBySeconds(60 * minutes);
        }
        public List<SymbolLog> GetSymbolLogsBySeconds(int seconds)
        {
            isLocked = true;

            var symbolLogs_ = new List<SymbolLog>();
            for (int i = symbolLogs.Count - 1; i >= 0; i--)
            {
                if ((DateTime.Now - symbolLogs[i].Date).TotalSeconds <= seconds)
                    symbolLogs_.Add(symbolLogs[i]);
                else
                    break;
            }

            isLocked = false;

            return symbolLogs_;
        }
        public List<SymbolLog> GetSymbolLogsByMilliSeconds(int milliseconds)
        {
            isLocked = true;

            var symbolLogs_ = new List<SymbolLog>();
            for (int i = symbolLogs.Count - 1; i >= 0; i--)
            {
                if ((DateTime.Now - symbolLogs[i].Date).TotalMilliseconds <= milliseconds)
                    symbolLogs_.Add(symbolLogs[i]);
                else
                    break;
            }

            isLocked = false;

            return symbolLogs_;
        }

        #endregion
    }
}
