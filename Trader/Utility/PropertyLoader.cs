using Newtonsoft.Json.Linq;
using Quantum.Framework.GenericProperties.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Utility
{
    public class PropertyLoader
    {
        public static void Save(Dictionary<string, GenericPropertyCollection> properties, string fileName = "properties.json")
        {
            var jArrayProperties = new JArray();

            foreach (var tradingServiceName in properties.Keys)
            {
                var jArrayTradingServiceProperties = GenericPropertySerializer.SerializePropertiesToArray(properties[tradingServiceName]);
                var jObjectProperties = new JObject()
                {
                    ["tradingServiceName"] = tradingServiceName,
                    ["properties"] = jArrayTradingServiceProperties
                };
                jArrayProperties.Add(jObjectProperties);
            }

            File.WriteAllText(fileName, jArrayProperties.ToString());
        }
    }
}
