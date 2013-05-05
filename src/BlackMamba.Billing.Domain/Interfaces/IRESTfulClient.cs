using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StructureMap;

namespace BlackMamba.Billing.Domain
{
    [PluginFamily(IsSingleton = true)]
    public interface IRESTfulClient
    {
        string Get(string url, int timeoutMilliSeconds);
    }
}
