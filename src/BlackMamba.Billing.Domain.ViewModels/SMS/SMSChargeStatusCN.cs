using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Domain.ViewModels.SMS
{
    public enum SMSChargeStatusCN
    {
        /// <summary>
        /// 自从上一次接口调用，客户端状态未改变
        /// </summary>
        不变,

        /// <summary>
        /// 刚下发client通道信息
        /// </summary>
        初始化,

        /// <summary>
        /// client已经发送充值短信
        /// </summary>
        已发送,
        /// <summary>
        /// client收到二次确认短信
        /// </summary>
        收到二次确认,
        /// <summary>
        /// client已经发送二次确认短信
        /// </summary>
        发送二次确认,
        /// <summary>
        /// client已经收到最终确认短信
        /// </summary>
        充值成功,
        /// <summary>
        /// SP 回调确认
        /// </summary>
        SP已回调  // sealed ，can not be replaced
    }
}
