using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Models.Payments
{
    public enum ReissueOrderStatus
    {
        // 补单成功
        Successed = 1,
        //订单未支付
        NonPament = 2,
        //已取消的订单
        CancledOrder = 3,
        //无此订单信息
        UnknownOrder = 4,
        //未处理
        Untreated = 5
    }
}
