using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Models.Billing
{
    [Serializable]
    public class CardPaymentCallbackResult : RedisModelBase
    {
        public string OrderNo { get; set; }

        public int ResultCode { get; set; }

        public float Amount { get; set; }

        public string Desc { get; set; }
    }
}
