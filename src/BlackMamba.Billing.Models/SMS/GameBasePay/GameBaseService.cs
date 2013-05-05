using System;
using SubSonic.Oracle.SqlGeneration.Schema;
using BlackMamba.Framework.SubSonic.Oracle;

namespace BlackMamba.Billing.Models.SMS
{

    /// <summary>
    /// 游戏基地充值
    /// </summary>
    [Serializable]
    [SubSonicTableNameOverride("GAMEBASE_SERVICE")]
    public class GameBaseService : ServiceBase
    {
        /// <summary>
        /// 请求充值URL
        /// </summary>
        [SubSonicColumnNameOverride("CHARGE_URL")]
        public string ChargeUrl { get; set; }

    }
}
