using System;
using SubSonic.Oracle.SqlGeneration.Schema;
using BlackMamba.Framework.SubSonic.Oracle;

namespace BlackMamba.Billing.Models.Payments
{
    [Serializable]
    [MonthPartitionSetting("CREATED_DATE", "BD", "BUSINESSDATA")]
    [SubSonicTableNameOverride("ORDER_LINES_INDE")]
    public class OrderLine : EntityBase
    {
        [SubSonicNullString]
        [SubSonicColumnNameOverride("CHARGE_TYPE")]
        public string ChargeType { get; set; } //r0_Cmd

        [SubSonicNullString]
        [SubSonicColumnNameOverride("PAYMENT_STATUS")]
        public string PaymentStatus { get; set; }//固定值"SUCCESS"表示支付成功 //REFUND 表示退款 r1_Code

        [SubSonicIndexMapping]
        [SubSonicNullString]
        [SubSonicColumnNameOverride("YEEPAY_PAY_NO")]
        public string YeepayPayNo { get; set; } // p4_FrpId +  p5_CardNo

        [SubSonicColumnNameOverride("PAYMENT_AMOUNT")]
        public float PaymentAmount { get; set; } //支付金额  p3_Amt

        public string Currency { get; set; } //p4_FrpId

        [SubSonicNullString]
        [SubSonicColumnNameOverride("PRODUCT_NAME")]
        public string ProductName { get; set; }

        [SubSonicColumnNameOverride("ORDER_NO")]
        public string OrderNo { get; set; } //p2_Order

        [SubSonicNullString]
        [SubSonicColumnNameOverride("YEEPAY_UID")]
        public string YeepayUId { get; set; }

        [SubSonicNullString]
        [SubSonicColumnNameOverride("BANK_ORDER_ID")]
        public string  BankOrderId { get; set; }

        [SubSonicColumnNameOverride("PAYMENT_DATE")]
        public DateTime? PaymentDate { get; set; }

        [SubSonicNullString]
        [SubSonicColumnNameOverride("CARD_NO")]
        public string CardNo { get; set; } //神州行卡号 //p5_CardNo

        [SubSonicColumnNameOverride("PAY_NOTICE_TIME")]
        public DateTime? PaymentNoticeTime { get; set; }

        [SubSonicNullString]
        [SubSonicColumnNameOverride("DESCRIPTION")]
        public string Description { get; set; }

        [SubSonicNullString]
        [SubSonicColumnNameOverride("CP_CALLBACK_STATUS")]
        public CardPaymentCallBackStatus CardStatus { get; set; } //p8_cardStatus 卡状态组

        [SubSonicColumnNameOverride("CONFIRM_AMOUNT")]
        public float? ConfirmAmount { get; set; } //p6_confirmAmount

        [SubSonicColumnNameOverride("REAL_AMOUNT")]
        public float? RealAmount { get; set; } // p7_realAmount

        [SubSonicNullString]
        [SubSonicColumnNameOverride("MERCHANT_CODE")]
        public string MerchantCode { get; set; } // p1_MerId 商户编号

        [SubSonicNullString]
        [SubSonicColumnNameOverride("EXPAND_INFO")]
        public string ExpandInfo { get; set; } //p9_MP 扩展信息

        [SubSonicColumnNameOverride("BALANCE_AMOUNT")]
        public float? BalanceAmount { get; set; } //pb_BalanceAmt  支付余额

        [SubSonicNullString]
        [SubSonicColumnNameOverride("BALANCE_ACT")]
        public string BalanceAct { get; set; } //pc_BalanceAct  余额卡号

        [SubSonicNullString]
        [SubSonicColumnNameOverride("BATCH_NO")]
        public string BatchNo { get; set; } // p1_MerId 商户编号

        [SubSonicColumnNameOverride("CALLBACK_TYPE")]
        public int CallbackType { get; set; } //p6_confirmAmount

        [SubSonicNullString]
        [SubSonicColumnNameOverride("CHANNEL_NO")]
        public string ChannelNo { get; set; } // p1_MerId 通道编号

        [SubSonicColumnNameOverride("NOTIFY_DATE")]
        public DateTime? NotifyDate { get; set; } // ru_Trxtime 交易结果通知时间

        public float? Price { get; set; }

        public float? Discount { get; set; }

        public int? Quantity { get; set; }

        [SubSonicColumnNameOverride("TOTAL_FEE_ADJUST")]
        public bool? IsTotalFeeAdjust { get; set; }

        [SubSonicColumnNameOverride("USE_COUPON")]
        public bool? UseCoupon { get; set; }
    }
}
