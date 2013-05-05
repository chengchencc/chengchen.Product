using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Models.Payments
{
    public enum OrderStatus
    {
        Initial = 0,
        // 订单已创建,等待用户处理
        Created = 1,
        // 从第三方充值流程全部成功
        Successed = 2,
        Failed = 3,
        Canceled = 4,
        Expired = 5,
        // 平台已接受充值,未加值到游戏
        Processing = 6,
        // 平台已接受充值,加值到游戏未成功.充值金额留存于平台
        Stoped = 7,
        //平台充值超过订单
        Exceed = 8,
        //部分充值
        PartlySuccess = 9,
        //退款订单
        Refunded = 10,
        //充值流程完全成功，并且已通知游戏服务器
        Complete = 11,
        //无效订单
        Invalid = 12
    }
}
