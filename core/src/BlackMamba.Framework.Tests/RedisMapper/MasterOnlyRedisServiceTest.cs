using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using BlackMamba.Framework.Core;
using BlackMamba.Framework.RedisMapper;

namespace Framework.Core.Tests.RedisMapper
{
    public class MasterOnlyRedisServiceTest
    {
        [Fact(Skip="Demo")]
        public void master_only_should_use_master_only()
        {
            SingletonBase<ConfigurableSet>.Instance[AppConfigKeys.REDIS_READ_WRITE_SERVERS] = "127.0.0.1:6379";
            SingletonBase<ConfigurableSet>.Instance[AppConfigKeys.REDIS_READONLY_SERVERS] = "127.0.0.1:6379;127.0.0.1:6377";

            var redis = new MasterOnlyRedisService();

            var p = new Person { Id = "1", Name = "hyperion" };
            redis.Add<Person>(p);
            redis.Delete<Person>(p);
        }

        public class Person : IRedisModel
        {
            public string Name { get; set; }

            public DateTime CreateDateTime { get; set; }

            public string ModuleName
            {
                get;
                set;
            }

            public string Id { get; set; }
        }
    }
}
