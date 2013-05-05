using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Framework.RedisMapper
{
    public interface IRedisModelWithSubModel
    {
        Dictionary<string, object> CustomProperties { get; set; }
    }
}
