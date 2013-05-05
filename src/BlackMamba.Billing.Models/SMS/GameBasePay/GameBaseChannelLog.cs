using System;
using SubSonic.Oracle.SqlGeneration.Schema;
using BlackMamba.Framework.SubSonic.Oracle;

namespace BlackMamba.Billing.Models.SMS
{
    public enum GameBaseChargeStatus
    {
        /// <summary>
        /// 最终确认成功
        /// </summary>
        Success,
        /// <summary>
        /// 充值过程中，还有下一个URL需要请求
        /// </summary>
        Processing,
        /// <summary>
        /// 已收到和预期不符的response
        /// </summary>
        Fail
    }

    [Serializable]
    [SubSonicTableNameOverride("GAMEBASE_CHANNEL_LOG")]
    public class GameBaseChannelLog : ChannelLogBase
    {

        /// <summary>
        /// 充值URL
        /// </summary>
        [SubSonicColumnNameOverride("REQUESTED_URL")]
        public string RequestedUrl { get; set; }

        /// <summary>
        /// 错误信息 (没有匹配到下一次请求的URL 或 最终成功信息未匹配)
        /// </summary>
        [SubSonicNullString]
        [SubSonicLongString]
        [SubSonicColumnNameOverride("ERROR_RESPONSE")]
        public string ErrorResponse { get; set; }


        [SubSonicColumnNameOverride("CHARGE_STATUS")]
        public GameBaseChargeStatus ChargeStatus { get; set; }
    }
}
