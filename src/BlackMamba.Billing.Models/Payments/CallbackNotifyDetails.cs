using System;
using SubSonic.Oracle.SqlGeneration.Schema;
using BlackMamba.Framework.SubSonic.Oracle;

namespace BlackMamba.Billing.Models.Payments
{
    [Serializable]
    [MonthPartitionSetting("CREATED_DATE", "BD", "BUSINESSDATA")]
    [SubSonicTableNameOverride("CALLBACK_NOTIFY_INDE")]
    public class CallbackNotification : EntityBase
    {
        [SubSonicIndexMapping]
        [SubSonicColumnNameOverride("ORDER_NO")]
        [SubSonicNullString]
        public string OrderNo { get; set; }

        [SubSonicColumnNameOverride("RESULT_CODE")]
        public int ResultCode { get; set; }

        [SubSonicColumnNameOverride("REQUEST_AMOUNT")]
        public float RequestAmount { get; set; }

        [SubSonicColumnNameOverride("SUCCESS_AMOUNT")]
        public float SuccessAmount { get; set; }

        [SubSonicLongString]
        [SubSonicNullString]
        public string Description { get; set; }

        [SubSonicStringLength(1024)]
        [SubSonicNullString]
        [SubSonicColumnNameOverride("CALLBACK_URL")]
        public string CallbackURL { get; set; }

        [SubSonicColumnNameOverride("LAST_REQUEST_DATE")]
        public DateTime LastRequestDate { get; set; }

        [SubSonicColumnNameOverride("IS_NOTIFY_SUCCESS")]
        public bool IsNotifySuccess { get; set; }

        [SubSonicColumnNameOverride("OUT_ORDER_NO")]
        [SubSonicNullString]
        public string OutOrderNo { get; set; }
    }
}
