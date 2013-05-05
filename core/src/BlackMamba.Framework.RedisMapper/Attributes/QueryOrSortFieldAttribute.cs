using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Framework.RedisMapper
{
    [AttributeUsage(AttributeTargets.Property)]
    public class QueryOrSortFieldAttribute : Attribute
    {

    }
}
