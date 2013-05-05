using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace BlackMamba.Billing.Domain
{
    public class LogHelper
    {
        public static void WriteInfo(string content, ConsoleColor color = ConsoleColor.Gray)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(content);

            LogManager.GetLogger("InfoLogger").Info(content);
            Console.ForegroundColor = oldColor;
        }

        public static void WriteError(string content)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(content);

            LogManager.GetLogger("ErrorLogger").Error(content);
            Console.ForegroundColor = oldColor;

        }
    }
}
