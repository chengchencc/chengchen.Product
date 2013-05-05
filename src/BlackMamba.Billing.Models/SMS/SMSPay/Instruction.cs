using System;
using SubSonic.Oracle.SqlGeneration.Schema;
using BlackMamba.Framework.SubSonic.Oracle;

namespace BlackMamba.Billing.Models.SMS
{
    /// <summary>
    /// 一个特服号 （ServiceNumber）对应多个指令 （Instruction）
    /// </summary>
    [Serializable]
    [SubSonicTableNameOverride("INSTRUCTION")]
    public class Instruction : EntityBase
    {
        [SubSonicColumnNameOverride("SERVICE_ID")]
        public int ServiceId { get; set; }

        /// <summary>
        /// 指令代码 
        /// </summary>
        [SubSonicColumnNameOverride("CODE")]
        public string Code { get; set; }

        [SubSonicColumnNameOverride("AMOUNT")]
        public float Amount { get; set; }

        /// <summary>
        /// 是否是模糊指令
        /// </summary>
        [SubSonicColumnNameOverride("IS_FUZZY")]
        public bool IsFuzzy { get; set; }

        [SubSonicColumnNameOverride("FUZZY_LENGTH")]
        public int? FuzzyLength { get; set; }
    }
}
