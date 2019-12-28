using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Data
{
    public class LastTimeSpan
    {
        public int Minutes { get; set; }
        public string DisplayName { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }

        public static List<LastTimeSpan> GetLastTimeSpans()
        {
            return new List<LastTimeSpan>()
            {
                new LastTimeSpan()
                {
                    DisplayName = "1 Minute",
                    Minutes = 1
                },new LastTimeSpan()
                {
                    DisplayName = "3 Minute",
                    Minutes = 3
                },new LastTimeSpan()
                {
                    DisplayName = "5 Minutes",
                    Minutes = 5
                },new LastTimeSpan()
                {
                    DisplayName = "15 Minutes",
                    Minutes = 15
                },new LastTimeSpan()
                {
                    DisplayName = "30 Minutes",
                    Minutes = 30
                },new LastTimeSpan()
                {
                    DisplayName = "1 Hour",
                    Minutes = 60
                },new LastTimeSpan()
                {
                    DisplayName = "2 Hours",
                    Minutes = 120
                },new LastTimeSpan()
                {
                    DisplayName = "6 Hours",
                    Minutes = 360
                },new LastTimeSpan()
                {
                    DisplayName = "12 Hours",
                    Minutes = 720
                },new LastTimeSpan()
                {
                    DisplayName = "1 Day",
                    Minutes = 1440
                },new LastTimeSpan()
                {
                    DisplayName = "2 Days",
                    Minutes = 2880
                },new LastTimeSpan()
                {
                    DisplayName = "From Beginning",
                    Minutes = 0
                }
            };
        }
    }
}
