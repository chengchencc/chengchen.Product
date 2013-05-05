using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watcher.Core.Interface;

namespace BlackMamba.Framework.Automation
{
    public class ConsoleDisplay : IDisplay
    {
        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}
