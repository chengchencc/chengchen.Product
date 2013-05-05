using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Models.Payments
{
    public enum PaymentStatus
    {
        // 已经支付成功
        SUCCESS = 1,
        //订单未支付
        INIT = 2,
        //订单已取消
        CANCELED = 3,
        //退订
        REFUNDED = 4,
        //支付失败
        FAILED = 5 
    }
}
