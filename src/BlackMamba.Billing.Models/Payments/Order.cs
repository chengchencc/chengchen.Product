using System;
using SubSonic.Oracle.SqlGeneration.Schema;
using BlackMamba.Framework.SubSonic.Oracle;

namespace BlackMamba.Billing.Models.Payments
{
    [Serializable]
    [MonthPartitionSetting("CREATED_DATE", "BD", "BUSINESSDATA")]
    [SubSonicTableNameOverride("ORDERS_INDE")]
    public class Order : EntityBase
    {
        // should add unique index
        [SubSonicIndexMapping]
        [SubSonicColumnNameOverride("ORDER_NO")]
        public string OrderNo { get; set; }

        [SubSonicIndexMapping]
        public OrderStatus OrderStatus { get; set; }//支付状态 0： 只提交订单 1： 提交并支付成功

        private CardPaymentRequestStatus cardPaymentRequestStatus = CardPaymentRequestStatus.NotRequest;
        [SubSonicColumnNameOverride("CP_REQUEST_STATUS")]
        public CardPaymentRequestStatus CardPaymentRequestStatus 
        { 
            get
            {
                return cardPaymentRequestStatus;
            }
            set
            {
                cardPaymentRequestStatus = value;
            }
        } // 1 :代表提交成功, 0 : 还没请求


        private CardPaymentCallBackStatus cardPaymentCallBackStatus = CardPaymentCallBackStatus.NotCallBack;
        [SubSonicColumnNameOverride("CP_CALLBACK_STATUS")]
        public CardPaymentCallBackStatus CardPaymentCallBackStatus 
        {
            get
            {
                return cardPaymentCallBackStatus;
            }
            set
            {
                cardPaymentCallBackStatus = value;
            }
        }// 0 : 代表提交成功, -1 ： 还没回调

        public float Amount { get; set; }

        private string _currency = "CNY";
        public string Currency
        {
            get { return _currency; }
            set { _currency = value; }
        }

        [SubSonicColumnNameOverride("PAYED_AMOUNT")]
        public float PayedAmount { get; set; }

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
        [SubSonicColumnNameOverride("MERCHANT_DESC")]
        public string MerchantExtentionalDescription { get; set; }

        [SubSonicStringLength(1024)]
        [SubSonicNullString]
        [SubSonicColumnNameOverride("CALL_BACK_URL")]
        public string CallBackUrl { get; set; }

        [SubSonicStringLength(64)]
        [SubSonicNullString]
        [SubSonicColumnNameOverride("USER_ID")]
        [SubSonicIndexMapping]
        public string UserId { get; set; }

        [SubSonicStringLength(64)]
        [SubSonicNullString]
        [SubSonicColumnNameOverride("USER_NAME")]
        public string UserName { get; set; }

        [SubSonicColumnNameOverride("CARD_TYPE")]
        public int? CardType { get; set; }

        [SubSonicColumnNameOverride("CARD_NO")]
        [SubSonicNullString]
        public string CardNo { get; set; }

        [SubSonicColumnNameOverride("CARD_PASSWORD")]
        [SubSonicNullString]
        public string CardPassword { get; set; }

        [SubSonicColumnNameOverride("CARD_AMOUNT")]
        public float? CardAmount { get; set; }

        [SubSonicColumnNameOverride("PAYMENT_TYPE")]
        public PaymentType PaymentType { get; set; }

        [SubSonicColumnNameOverride("CHECK_COUNT")]
        public int CheckCount { get; set; }

        [SubSonicColumnNameOverride("OUT_ORDER_NO"), SubSonicNullString]
        public string OutOrderNo { get; set; }

        [SubSonicColumnNameOverride("PARTENER_NO"),SubSonicNullString]
        public string PartenerNo { get; set; }


    }
}
