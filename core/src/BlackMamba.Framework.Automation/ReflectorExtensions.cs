using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Framework.Automation
{
    internal static class ReflectorExtensions
    {
        public static bool IsXmlGeneratableType(this Type type)
        {
            return !type.IsAbstract && type.GetInterfaces().Contains(typeof(IXmlGeneratable));
        }
    }
}
