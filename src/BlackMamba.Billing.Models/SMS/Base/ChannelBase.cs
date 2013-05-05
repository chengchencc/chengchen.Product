using BlackMamba.Framework.SubSonic.Oracle;
using SubSonic.Oracle.SqlGeneration.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Models.SMS
{
    [Serializable]
    public abstract class ChannelBase : EntityBase
    {
        /// <summary>
        /// FK to SMSService
        /// </summary>
        [SubSonicColumnNameOverride("SERVICE_ID")]
        public int ServiceId { get; set; }
    }
}
