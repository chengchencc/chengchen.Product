using BlackMamba.Framework.SubSonic.Oracle;
using SubSonic.Oracle.SqlGeneration.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Models
{

    [SubSonicTableNameOverride("SETTING_IN_REGION")]
    public class SettingInRegion : EntityBase
    {
        [SubSonicColumnNameOverride("SETTING_ID")]
        public int SettingId { get; set; }

        /// <summary>
        /// NULL 表示所有省都可以使用
        /// </summary>
        [SubSonicColumnNameOverride("PROVINCE_ID")]
        public int? ProvinceId { get; set; }

        /// <summary>
        /// NULL 表示所有市都可以使用
        /// </summary>
        [SubSonicColumnNameOverride("CITY_ID")]
        public int? CityId { get; set; }

        [SubSonicColumnNameOverride("IS_BLACK_LIST")]
        public bool IsBlackList { get; set; }

    }
}
