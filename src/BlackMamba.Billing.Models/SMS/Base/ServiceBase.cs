using System;
using SubSonic.Oracle.SqlGeneration.Schema;
using BlackMamba.Framework.SubSonic.Oracle;
using System.ComponentModel;

namespace BlackMamba.Billing.Models.SMS
{
    [Serializable]
    public abstract class ServiceBase : EntityBase
    {
        [SubSonicColumnNameOverride("SP_ID")]
        public int SpId { get; set; }

        [DisplayName("通道类型")]
        [SubSonicColumnNameOverride("SERVICE_TYPE")]
        public ServiceType Type { get; set; }
    }
}
