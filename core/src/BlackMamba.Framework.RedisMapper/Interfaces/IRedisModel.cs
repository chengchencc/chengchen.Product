﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Framework.RedisMapper
{
    public interface IRedisModel : IRedisModelBase
    {
        DateTime CreateDateTime { get; set; }

        string ModuleName { get; set; }
    }
}
