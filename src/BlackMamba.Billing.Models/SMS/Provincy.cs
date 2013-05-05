using BlackMamba.Framework.SubSonic.Oracle;
using SubSonic.Oracle.SqlGeneration.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Models.SMS
{
    [Serializable]
    [SubSonicTableNameOverride("PROVINCE")]
    public class Province
    {
        [SubSonicPrimaryKey]
        public int Id  { get; set; }

        [SubSonicColumnNameOverride("NAME")]
        public string Name { get; set; }

    }
}
