using BlackMamba.Framework.SubSonic.Oracle;
using SubSonic.Oracle.SqlGeneration.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Models.SMS
{
    [Serializable]
    [SubSonicTableNameOverride("SMS_CHANNEL")]
    public class SMSChannel : ChannelBase
    {

        /// <summary>
        /// 特服号(冗余字段)
        /// </summary>
        [SubSonicColumnNameOverride("SERVICE_NUMBER")]
        public string ServiceNumber { get; set; }

        /// <summary>
        /// FK to Instruction
        /// assign it only if this setting is only applied on a special instruction
        /// </summary>
        [SubSonicColumnNameOverride("INSTRUCTION_ID")]
        public int? InstructionId { get; set; }


        /// <summary>
        /// 请求充值信息模板，例如： 请发送指令 {0} 到 {1} 按照短信提示进行支付
        /// </summary>
        [SubSonicNullString]
        [SubSonicColumnNameOverride("CHARGE_MSG_TPL")]
        public string ChargeMessageTemplate { get; set; }

        /// <summary>
        /// 请求充值信息正则表达式，用于取到 指令 和 号码 ?
        /// </summary>
        [SubSonicNullString]
        [SubSonicColumnNameOverride("CHARGE_MSG_REGEX")]
        public string ChargeMessageRegex { get; set; }

        /// <summary>
        /// 二次确认信息内容，例如： Y
        /// </summary>
        [SubSonicNullString]
        [SubSonicColumnNameOverride("CONFIRM_MSG")]
        public string ConfirmMessage { get; set; }

        /// <summary>
        /// 二次确认信息模板，例如： 回复短信“{0}”确认订购。
        /// </summary>
        [SubSonicNullString]
        [SubSonicColumnNameOverride("CONFIRM_MSG_TPL")]
        public string ConfirmMessageTemplate { get; set; }

        /// <summary>
        /// 最终订购成功确认信息，例如： 欢迎您订购XXX产品。
        /// </summary>
        [SubSonicNullString]
        [SubSonicColumnNameOverride("FINAL_MSG")]
        public string FinalConfirmMessage { get; set; }

        [SubSonicNullString]
        [SubSonicColumnNameOverride("CONFIRM_NO_REG")]
        public string ConfirmNoRegex { get; set; }

        [SubSonicNullString]
        [SubSonicColumnNameOverride("Final_NO_REG")]
        public string FinalConfirmNoRegex { get; set; }
    }
}
