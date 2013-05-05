using System;
using SubSonic.Oracle.SqlGeneration.Schema;
using BlackMamba.Framework.SubSonic.Oracle;
using System.ComponentModel;

namespace BlackMamba.Billing.Models.SMS
{

    /// <summary>
    /// 短代特服号充值
    /// 一个SP 对应多个通道， 一个通道有一个特服号
    /// </summary>
    [Serializable]
    [SubSonicTableNameOverride("SMS_SERVICE")]
    public class ShortMessageService : ServiceBase
    {
        /// <summary>
        /// 特服号
        /// </summary>
        [DisplayName("特服号")]
        [SubSonicColumnNameOverride("SERVICE_NUMBER")]
        public string ServiceNumber { get; set; }

        [DisplayName("名称")]
        [SubSonicColumnNameOverride("Name")]
        public string Name { get; set; }

        [DisplayName("是否手动维护")]
        [SubSonicColumnNameOverride("IS_MANULLY")]
        public bool IsManully { get; set; }

        /// <summary>
        /// 电信是否可用
        /// </summary>
        [SubSonicColumnNameOverride("IS_TELCOM")]
        public bool IsTelcom { get; set; }

        /// <summary>
        /// 移动是否可用
        /// </summary>
        [SubSonicColumnNameOverride("IS_MOBILE")]
        public bool IsMobile { get; set; }

        /// <summary>
        /// 联通是否可用
        /// </summary>
        [SubSonicColumnNameOverride("IS_UNICOM")]
        public bool IsUnicom { get; set; }

    }
}
