using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Framework.RedisMapper
{
    public class AppConfigKeys
    {
        public const string REDIS_READ_WRITE_SERVERS = "redis_read_write_servers";

        public const string REDIS_READONLY_SERVERS = "redis_readonly_servers";

        public const string MAX_WRITE_POOL_SIZE = "MaxWritePoolSize";

        public const string MAX_READ_POOL_SIZE = "MaxReadPoolSize";

        public const string CACHE_REDIS_READ_WRITE_SERVERS = "Cache_redis_read_write_servers";

        public const string CACHE_REDIS_READONLY_SERVERS = "Cache_redis_readonly_servers";

        public const string CACHE_MAX_WRITE_POOL_SIZE = "Cache_MaxWritePoolSize";

        public const string CACHE_MAX_READ_POOL_SIZE = "Cache_MaxReadPoolSize";

        public const string LOG_REDIS_READ_WRITE_SERVERS = "Log_redis_read_write_servers";

        public const string LOG_REDIS_READONLY_SERVERS = "Log_redis_readonly_servers";

        public const string LOG_MAX_WRITE_POOL_SIZE = "Log_MaxWritePoolSize";

        public const string LOG_MAX_READ_POOL_SIZE = "Log_MaxReadPoolSize";

        public const string ALLOWED_FILE_TYPES = "AllowedFileTypes";
    }
}
