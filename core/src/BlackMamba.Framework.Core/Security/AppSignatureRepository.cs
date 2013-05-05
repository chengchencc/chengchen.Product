using System;
using System.Collections.Generic;
using BlackMamba.Framework.Core;

namespace BlackMamba.Framework.Core.Security
{
    public class AppSignatureRepository : SingletonBase<AppSignatureRepository>
    {
        public AppSignatureRepository()
        {
            SecretKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // youle 
            SecretKeys["yl"] = "4c6f1851f6a7496c86ffac67540ae9f5";
            SecretKeys["ylchs"] = "e1666f081f6045bf96197f966bf54a06";

            //
            /* reserved keys
             * 
             * 014a6aa8d2c142268f17d8b58378372f
             * ac802d2632fb44c893b4ca1477721fff
             * ea9f0c8855984c578cbe6b232282bf5e
             * febb604864ce49278a5e21ae2453ba95
             * 
             */
        }

        public Dictionary<string, string> SecretKeys { get; private set; }

        public string TryGet(string appType)
        {
            if (!appType.IsNullOrEmpty() && this.SecretKeys.ContainsKey(appType))
            {
                return this.SecretKeys[appType];
            }

            return string.Empty;
        }
    }
}
