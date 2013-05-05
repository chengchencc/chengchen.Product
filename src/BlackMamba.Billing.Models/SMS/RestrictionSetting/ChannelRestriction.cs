using BlackMamba.Framework.SubSonic.Oracle;
using SubSonic.Oracle.SqlGeneration.Schema;
using System;

namespace BlackMamba.Billing.Models
{
    public enum RestrictionType
    {
        Globally = 0, // 整个通道限制
        EachUser,   // 每个用户限制
    }

    public enum RestrictionByTimeSpan
    {
        Globally = 0,  // 没有时间段限制
        Daily = 1,
        Monthly = 2,
    }

    public enum Operators
    {
        Globally = 0, // 没有运营商限制
        ChinaMobile,
        ChinaUnicom,
        ChinaTelecom
    }

    [Serializable]
    [SubSonicTableNameOverride("CHANNEL_RESTRIC")]
    public class ChannelRestriction : EntityBase
    {
        [SubSonicColumnNameOverride("CHANNEL_ID")]
        public int ChannelId { get; set; }

        /// <summary>
        /// FK to Instruction
        /// assign it only if this setting is only applied on a special instruction
        /// </summary>
        [SubSonicColumnNameOverride("INSTRUCTION_ID")]
        public int? InstructionId { get; set; }

        [SubSonicColumnNameOverride("RESTRIC_TYPE")]
        public RestrictionType Type { get; set; }

        /// <summary>
        /// NULL 表示没有省限制
        /// </summary>
        [SubSonicColumnNameOverride("PROVINCE_ID")]
        public int? ProvinceId { get; set; }

        /// <summary>
        /// NULL 表示没有城市限制
        /// </summary>
        [SubSonicColumnNameOverride("CITY_ID")]
        public int? CityId { get; set; }

        /// <summary>
        /// 运营商限制
        /// </summary>
        [SubSonicColumnNameOverride("OP")]
        public Operators Operator { get; set; }

        /// <summary>
        /// 0    表示禁用
        /// </summary>
        [SubSonicColumnNameOverride("MAX_TIMES")]
        public int MaxTimes { get; set; }

        [SubSonicColumnNameOverride("TIME_SPAN")]
        public RestrictionByTimeSpan TimeSpan { get; set; }

       
    }
}
