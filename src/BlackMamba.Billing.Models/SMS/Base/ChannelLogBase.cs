using System;
using SubSonic.Oracle.SqlGeneration.Schema;
using BlackMamba.Framework.SubSonic.Oracle;
using System.ComponentModel;

namespace BlackMamba.Billing.Models.SMS
{
    [Serializable]
    public abstract class ChannelLogBase : LogBase
    {
        [DisplayName("手机号码")]
        [SubSonicNullString]
        [SubSonicColumnNameOverride("MOBILE")]
        public string Mobile { get; set; }

        [DisplayName("IMSI")]
        [SubSonicNullString]
        [SubSonicColumnNameOverride("IMSI")]
        public string IMSI { get; set; }

        [SubSonicColumnNameOverride("PROVINCE_ID")]
        [DisplayName("省份编号")]
        public int? ProvinceId { get; set; }

        [DisplayName("城市编号")]
        [SubSonicColumnNameOverride("CITY_ID")]
        public int? CityId { get; set; }

        [DisplayName("运营商编号")]
        [SubSonicColumnNameOverride("OP_ID")]
        public int? OpId { get; set; } //运营商

        [DisplayName("金额")]
        [SubSonicColumnNameOverride("Amount")]
        public float? Amount { get; set; }

        /// <summary>
        /// FK to Channel Id
        /// </summary>
        [DisplayName("通道编号")]
        [SubSonicColumnNameOverride("CHANNEL_ID")]
        public int? ChannelId { get; set; }
    }
}
