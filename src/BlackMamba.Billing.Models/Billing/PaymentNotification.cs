using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackMamba.Framework.RedisMapper;

namespace BlackMamba.Billing.Models
{

    [Serializable]
    public class PaymentNotification
    {
       [QueryOrSortField]
        public string OrderNo { get; set; }

        public int ResultCode { get; set; }

        public string OutOrderNo { get; set; }

        public float RequestAmount { get; set; }

        public float SuccessAmount { get; set; }

        public string Description { get; set; }

        public string CallbackURL { get; set; }

        public DateTime LastRequestDate { get; set; }

    }
}
