using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;


namespace EasyResX
{
    // configuration class
    public sealed class EasyResXConfiguration
    {
        // store the object
        private static EasyResXConfiguration cachedConfig;

        private EasyResXConfiguration()
        {
        }

        // get current configuration
        public static EasyResXConfiguration CurrentConfig 
        {
            get
            {
                if (cachedConfig == null)
                {
                    cachedConfig = new EasyResXConfiguration();
                }

                return cachedConfig;
            }
        }

        // folders to operate local member
        private string[] operatedFolders;

        // folders to operate
        public string[] OperatedFolders
        {
            get
            {
                if (this.operatedFolders == null)
                {
                    string commaseparated = ConfigurationManager.AppSettings.Get("operatedFolders");
                    this.operatedFolders = commaseparated.Split(
                        new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                }

                return this.operatedFolders;
            }
        }
    }
}