using System;
using SubSonic.Oracle.SqlGeneration.Schema;
using BlackMamba.Framework.SubSonic.Oracle;
using System.ComponentModel;

namespace BlackMamba.Billing.Models.SMS
{
    public enum SMSChargeStatus
    {
        /// <summary>
        /// 自从上一次接口调用，客户端状态未改变
        /// </summary>
        NotChange,

        /// <summary>
        /// 没有找到通道信息
        /// </summary>
        NotFound,

        /// <summary>
        /// 刚下发client通道信息
        /// </summary>
        Initial,

        /// <summary>
        /// client已经发送充值短信
        /// </summary>
        Sent,
        /// <summary>
        /// client收到二次确认短信
        /// </summary>
        ConfirmReceived,
        /// <summary>
        /// client已经发送二次确认短信
        /// </summary>
        ConfirmSent,
        /// <summary>
        /// client已经收到最终确认短信
        /// </summary>
        Success,
        /// <summary>
        /// SP 回调确认
        /// </summary>
        SPConfirmed  // sealed ，can not be replaced
   
    }

    [Serializable]
    [SubSonicTableNameOverride("SMS_CHANNEL_LOG")]
    public class SMSChannelLog : ChannelLogBase
    {
       
        /// <summary>
        /// 特服号(冗余字段)
        /// </summary>
        [DisplayName("特服号")]
        [SubSonicNullString]
        [SubSonicColumnNameOverride("SERVICE_NUMBER")]
        public string ServiceNumber { get; set; }

        /// <summary>
        /// FK to Instruction
        /// </summary>
        [DisplayName("指令编号")]
        [SubSonicColumnNameOverride("INSTRUCTION_ID")]
        public int? InstructionId { get; set; }

        /// <summary>
        /// 指令代码(冗余字段) 
        /// </summary>
        [SubSonicNullString]
        [DisplayName("指令")]
        [SubSonicColumnNameOverride("INSTRUCTION")]
        public string Instruction { get; set; }

        [SubSonicNullString]
        [SubSonicColumnNameOverride("ORDER_NO")]
        public string OrderNo { get; set; }

        [DisplayName("支付状态")]
        [SubSonicColumnNameOverride("CHARGE_STATUS")]
        public SMSChargeStatus ChargeStatus { get; set; }
    }
}
