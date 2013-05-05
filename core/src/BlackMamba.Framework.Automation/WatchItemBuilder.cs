using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Watcher.Core.Entity;

namespace BlackMamba.Framework.Automation
{
    public class WatchItemBuilder
    {
        public static WatchItem Build(SpecItem item)
        {
            var watchItem = new WatchItem();

            watchItem.Name = item.Name;
            watchItem.Verb = item.Method.ToString();
            watchItem.FixtureName = item.FixtureType.ToString() + "Fixture";
            watchItem.Address = new WatchAddress();
            watchItem.Address.Hosts = new List<Host>();
            watchItem.Address.Link = item.LinkUrl;
            watchItem.Params = new List<Param>();
            watchItem.Params.AddRange(item.Params);

            
            watchItem.Headers = new List<Header>();

            if (item.Headers.Count !=0)
            {
                watchItem.Headers.AddRange(item.Headers);               
            }

            foreach (var host in item.Hosts)
            {
                var h = new Host { Name = host.Name, Port = host.Port };

                watchItem.Address.Hosts.Add(h);
            }

            watchItem.Results = item.Results;

            return watchItem;
        }
    }
}
