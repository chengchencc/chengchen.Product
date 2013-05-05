using BlackMamba.Framework.SubSonic.Oracle;
using SubSonic.Oracle.SqlGeneration.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Models.SMS
{
    [Serializable]
    [SubSonicTableNameOverride("MOBILE_INFO")]
    public class MobileInfo
    {
        [SubSonicColumnNameOverride("MOBILE")]
        [SubSonicPrimaryKey]
        public string Mobile { get; set; }

        [SubSonicColumnNameOverride("PROVINCE_ID")]
        public int ProvinceId { get; set; }

        [SubSonicColumnNameOverride("CITY_ID")]
        public int CityId { get; set; }

        [SubSonicColumnNameOverride("DETAIL")]
        public string Detail { get; set; }

        [SubSonicColumnNameOverride("DISTRICTION")]
        public string Distriction { get; set; }

        [SubSonicColumnNameOverride("POST_CODE")]
        public string PostCode { get; set; }

        [SubSonicColumnNameOverride("OPERATOR")]
        public int OperatorId { get; set; }

    }
}
