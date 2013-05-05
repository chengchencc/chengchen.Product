using BlackMamba.Framework.SubSonic.Oracle;
using SubSonic.Oracle.SqlGeneration.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Models.SMS
{
    [Serializable]
    [SubSonicTableNameOverride("CITY")]
    public class City
    {
        [SubSonicPrimaryKey]
        public int Id { get; set; }

        [SubSonicColumnNameOverride("PROVINCE_ID")]
        public int ProvinceId { get; set; }

        [SubSonicColumnNameOverride("NAME")]
        public string Name { get; set; }

    }
}
