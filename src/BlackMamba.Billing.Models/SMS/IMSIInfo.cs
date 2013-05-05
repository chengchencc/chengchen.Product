using System;
using SubSonic.Oracle.SqlGeneration.Schema;
using BlackMamba.Framework.SubSonic.Oracle;

namespace BlackMamba.Billing.Models.SMS
{
    [Serializable]
    [SubSonicTableNameOverride("IMSI_INFO")]
    public class IMSIInfo : EntityBase
    {
        [SubSonicIndexMapping(false)]
        [SubSonicColumnNameOverride("IMSI")]
        public string IMSI { get; set; }

        [SubSonicIndexMapping(false)]
        [SubSonicColumnNameOverride("PHONE_NUMBER")]
        public string Mobile { get; set; }

        public override string ToString()
        {
            return string.Format("IMSI:{0},Mobile:{1};",IMSI,Mobile);
        }
    }
}
