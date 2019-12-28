using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trader.Utility;
using Trader.Utility.SettingsManager.Manager;

namespace Trader
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            GlobalSettings.StartDate = DateTime.Now;

            SettingsManager.Instance.AutoSave = true;
            SettingsManager.Instance.Load("settings.json");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Forms.FormMain());
        }
    }
}
