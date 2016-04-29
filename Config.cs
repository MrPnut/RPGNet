using System;
using System.Collections.Generic;
using System.IO;

namespace RPGNet
{
    class Config
    {
        private Dictionary<String, String> Data;
        public Config(String Location)
        {
            String[] Conf;
            Data = new Dictionary<string, string>();

            if (File.Exists(Location))
            {
                foreach (String Line in File.ReadAllLines(Location))
                {
                    Conf = Line.Split(':');
                    Data.Add(Conf[0].Trim(), Conf[2]);
                }
            }
            else
            {
                File.AppendAllText(Location, "ilasm: " + @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\ilasm.exe");
                Data.Add("ilasm", @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\ilasm.exe");
            }
        }

        public String getConf(String Key)
        {
            if (Data.ContainsKey(Key))
            {
                return Data[Key];
            }
            else
            {
                return "";
            }
        }
    }
}
