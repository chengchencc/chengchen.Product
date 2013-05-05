using System;
using SubSonic.Oracle.SqlGeneration.Schema;

namespace BlackMamba.Billing.Models.Payments
{
 //  [SubSonicTableNameOverride("UORDER")]
    public class CustomerOrder
    {
        [SubSonicPrimaryKey]
        public int Id { get; set; } //自增

        [SubSonicIndexMapping]
        public int CustomerId { get; set; }

        // should add unique index
        [SubSonicIndexMapping]
        public string OrderNo { get; set; }

        public OrderStatus Status { get; set; }//支付状态 0： 只提交订单 1： 提交并支付成功

        public float Amount { get; set; }

        private string _currency = "CNY";
        public string Currency
        {
            get { return _currency; }
            set { _currency = value; }
        }

        public float? Payed { get; set; }

        [SubSonicStringLength(20)]
        [SubSonicNullString]
        public string ProductName { get; set; }

        [SubSonicStringLength(20)]
        [SubSonicNullString]
        public string ProductType { get; set; }

        [SubSonicStringLength(20)]
        [SubSonicNullString]
        public string ProductDescription { get; set; }

        [SubSonicStringLength(200)]
        [SubSonicNullString]
       [SubSonicColumnNameOverride("merchantDes")]
        public string MerchantExtentionalDescription { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? PurchaseDate { get; set; }


    }
}
