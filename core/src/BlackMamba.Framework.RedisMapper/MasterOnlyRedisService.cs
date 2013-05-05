using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Framework.RedisMapper
{
    public class MasterOnlyRedisService : RedisService, IMasterOnlyRedisService
    {
        public override IRedisClientsManager RedisClientManager
        {
            get
            {
                return RedisClientManagerFactory.Instance.MasterOnlyClientManager;
            }
        }
    }
}
