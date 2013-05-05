using BlackMamba.Framework.SubSonic.Oracle;
using SubSonic.Oracle.SqlGeneration.Schema;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Models
{
    public enum SMSDirection
    {
        Send,
        Receive
    }

    /// <summary>
    /// User charge sms content.
    /// </summary>
    [Serializable]
    [SubSonicTableNameOverride("SMS_LOG")]
    public class SMSLog : LogBase
    {

        [DisplayName("通道充值日志编号")]
        [SubSonicColumnNameOverride("CHANNEL_LOG_ID")]
        public long ChannelLogId { get; set; }

        [DisplayName("内容")]
        [SubSonicColumnNameOverride("CONTENT")]
        [SubSonicNullString]
        public string Content { get; set; }

        [DisplayName("发送至号码")]
        [SubSonicColumnNameOverride("Target_Phone_No")]
        [SubSonicNullString]
        public string TargetPhoneNo { get; set; }

        /// <summary>
        /// 是否发送成功
        /// </summary>
        [DisplayName("是否成功发送")]
        [SubSonicColumnNameOverride("IS_SENT")]
        public bool IsSent { get; set; }

        [DisplayName("发送/接收")]
        [SubSonicColumnNameOverride("SMS_DIRECTION")]
        public SMSDirection Direction { get; set; }
    }
}
