using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watcher.Core.Entity;

namespace BlackMamba.Framework.Automation
{
    public class SpecHost
    {
        public SpecHost()
        {
            this.Port = 80;
        }

        public string Name { get; set; }

        /// <summary>
        /// Defalut value is 80
        /// </summary>
        public int Port { get; set; }
    }
}
