using System;
using SubSonic.Oracle.SqlGeneration.Schema;
using BlackMamba.Framework.SubSonic.Oracle;

namespace BlackMamba.Billing.Models.Payments
{
    [Serializable]
    [MonthPartitionSetting("CREATED_DATE", "BD", "BUSINESSDATA")]
    [SubSonicTableNameOverride("CARDPAY_RETRY_INDE")]
    public class CardPaymentRetry : EntityBase
    {
        // should add unique index
        [SubSonicIndexMapping]
        [SubSonicNullString]
        [SubSonicColumnNameOverride("ORDER_NO")]
        public string OrderNo { get; set; }

        [SubSonicIndexMapping]
        [SubSonicNullString]
        [SubSonicColumnNameOverride("ORDER_ID")]
        public long OrderID { get; set; }

        [SubSonicColumnNameOverride("CARD_TYPE")]
        [SubSonicNullString]
        public string CardType { get; set; }

        [SubSonicColumnNameOverride("CARD_NO")]
        [SubSonicNullString]
        public string CardNo { get; set; }

        [SubSonicColumnNameOverride("CARD_PASSWORD")]
        [SubSonicNullString]
        public string CardPassword { get; set; }

        private float cardAmount = 0.0f;
        [SubSonicColumnNameOverride("CARD_AMOUNT")]
        public float CardAmount
        {
            get
            {
                return cardAmount == 0.0f ? Amount : cardAmount;
            }
            set
            {
                cardAmount = value;
            }
        }

        public float Amount { get; set; }

        private bool verifyAmount = false;
        [SubSonicColumnNameOverride("VERIFY_AMOUNT")]
        public bool VerifyAmount
        {
            get
            {
                return verifyAmount;
            }
            set
            {
                verifyAmount = value;
            }
        }

        [SubSonicStringLength(30)]
        [SubSonicNullString]
        [SubSonicColumnNameOverride("PRODUCT_NAME")]
        public string ProductName { get; set; }

        [SubSonicStringLength(30)]
        [SubSonicNullString]
        [SubSonicColumnNameOverride("PRODUCT_TYPE")]
        public string ProductType { get; set; }

        [SubSonicStringLength(30)]
        [SubSonicNullString]
        [SubSonicColumnNameOverride("PRODUCT_DESC")]
        public string ProductDescription { get; set; }

        [SubSonicStringLength(200)]
        [SubSonicNullString]
        [SubSonicColumnNameOverride("MERCHANT_DES")]
        public string MerchantExtentionalDescription { get; set; }
    }
}
