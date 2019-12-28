using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Utility.Events
{
    public class FileLoggedEventArgs : EventArgs
    {
        public string Text { get; set; }
    }
}
