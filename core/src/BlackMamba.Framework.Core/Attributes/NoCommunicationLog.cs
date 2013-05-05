using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Framework.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class NoCommunicationLog : Attribute
    {
    }
}
