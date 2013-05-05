using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watcher.Core.Entity;

namespace BlackMamba.Framework.Automation
{
    public static class SpecItemExtensions
    {
        public static WatchItem ToWatchItem(this SpecItem spec)
        {
            return WatchItemBuilder.Build(spec);
        }
    }
}
