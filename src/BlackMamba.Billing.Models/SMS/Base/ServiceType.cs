using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Models.SMS
{
    public enum ServiceType
    {
        /// <summary>
        /// 用于上行短信，获取手机号
        /// </summary>
        SMS,
        /// <summary>
        /// 短代充值
        /// </summary>
        SMSCharge,
        /// <summary>
        /// 游戏基地充值
        /// </summary>
        GameBaseCharge,
    }
}
