using BlackMamba.Framework.RedisMapper;
using System;

namespace BlackMamba.Billing.Models
{
    [Serializable]
    public class RedisModelBase : IRedisModel
    {
        public string Id { get; set; }

        private DateTime _createDateTime = DateTime.Now;
        [QueryOrSortField]
        public DateTime CreateDateTime
        {
            get
            {
                return _createDateTime;
            }
            set
            {
                _createDateTime = value;
            }
        }

        [QueryOrSortField]
        public string ModuleName { get; set; }
    }
}
