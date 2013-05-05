using BlackMamba.Framework.SubSonic.Oracle;
using SubSonic.Oracle.SqlGeneration.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Models.SMS
{
    [Serializable]
    [SubSonicTableNameOverride("GAMEBASE_CHANNEL")]
    public class GameBaseChannel : ChannelBase
    {
        /// <summary>
        /// 请求充值信息正则表达式，用于取到 下一个充值的url
        /// </summary>
        [SubSonicNullString]
        [SubSonicColumnNameOverride("CHARGE_URL_REGEX")]
        public string ChargeURLRegex { get; set; }


        /// <summary>
        /// 下一个充值的url
        /// </summary>
        [SubSonicNullString]
        [SubSonicColumnNameOverride("CHARGE_URL")]
        public string ChargeURL { get; set; }

        [SubSonicColumnNameOverride("NEXT_CHANNEL_ID")]
        public int? NextChargeChannelId { get; set; }

        /// <summary>
        /// 匹配最终确认信息正则表达式，符合的话表示最终充值成功
        /// </summary>
        [SubSonicNullString]
        [SubSonicColumnNameOverride("CONFIRM_URL_REGEX")]
        public string ConfirmURLRegex { get; set; }


    }
}
