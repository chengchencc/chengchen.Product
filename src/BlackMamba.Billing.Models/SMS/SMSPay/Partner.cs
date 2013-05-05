using System;
using SubSonic.Oracle.SqlGeneration.Schema;
using BlackMamba.Framework.SubSonic.Oracle;

namespace BlackMamba.Billing.Models.SMS
{
    /// <summary>
    /// 商户回调地址存储
    /// </summary>
    [Serializable]
    [SubSonicTableNameOverride("PARTNER")]
    public class Partner : EntityBase
    {
        /// <summary>
        /// 商户帐号
        /// </summary>
        [SubSonicColumnNameOverride("PARTNER_NO")]
        public string PartnerNo { get; set; }

        /// <summary>
        /// 回调地址
        /// </summary>
        [SubSonicColumnNameOverride("CALLBACK_URL")]
        public string CallbackURL{ get; set; }
    }
}
