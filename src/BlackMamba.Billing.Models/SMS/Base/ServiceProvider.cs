using System;
using SubSonic.Oracle.SqlGeneration.Schema;
using BlackMamba.Framework.SubSonic.Oracle;

namespace BlackMamba.Billing.Models.SMS
{
    public enum DynamicSP
    {
        None = 0,
        CTU = 1
    }


    [Serializable]
    [SubSonicTableNameOverride("SERVICE_PROVIDER")]
    public class ServiceProvider : EntityBase
    {
        [SubSonicColumnNameOverride("NAME")]
        public string Name { get; set; }

        [SubSonicNullString]
        [SubSonicColumnNameOverride("DESCRIPTION")]
        public string Description { get; set; }

        [SubSonicColumnNameOverride("DYNAMIC_SP")]
        public DynamicSP DynamicSP { get; set; }
    }
}
