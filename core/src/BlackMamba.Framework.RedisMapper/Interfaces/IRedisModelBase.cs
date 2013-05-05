using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Framework.RedisMapper
{
    public interface IRedisModelBase
    {
        string Id { get; set; }
    }
}
