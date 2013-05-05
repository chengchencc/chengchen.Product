using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackMamba.Framework.Core;

namespace BlackMamba.Framework.RedisMapper
{
    public class RedisClientManagerFactory : SingletonBase<RedisClientManagerFactory>
    {
        public IRedisClientsManager MixedClientManager
        {
            get
            {
                if (_mixedClientManager == null)
                {
                    _mixedClientManager = GetInstance();
                }

                return _mixedClientManager;
            }
        } IRedisClientsManager _mixedClientManager;


        public IRedisClientsManager MasterOnlyClientManager
        {
            get
            {
                if (_masterOnlyClientManager == null)
                {
                    _masterOnlyClientManager = GetInstance(true);
                }

                return _masterOnlyClientManager;
            }
        } IRedisClientsManager _masterOnlyClientManager;

        private IRedisClientsManager GetInstance(bool masterOnly = false)
        {
            var redisClientManager = default(IRedisClientsManager);
            var redisConfig = new RedisClientManagerConfig
            {
                MaxWritePoolSize = AppConfigKeys.MAX_WRITE_POOL_SIZE.ConfigValue().ToInt32(),
                MaxReadPoolSize = AppConfigKeys.MAX_READ_POOL_SIZE.ConfigValue().ToInt32(),
                AutoStart = true
            };


            string[] readWriteHosts = AppConfigKeys.REDIS_READ_WRITE_SERVERS.ConfigValue().Split(';');
            string[] readOnlyHosts = AppConfigKeys.REDIS_READONLY_SERVERS.ConfigValue().Split(';');

            if (masterOnly)
            {
                redisClientManager = new PooledRedisClientManager(readWriteHosts, readWriteHosts, redisConfig);
            }
            else
            {
                redisClientManager = new PooledRedisClientManager(readWriteHosts, readOnlyHosts, redisConfig);
            }

            return redisClientManager;
        }
    }
}
