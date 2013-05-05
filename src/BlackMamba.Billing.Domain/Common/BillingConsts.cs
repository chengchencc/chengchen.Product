using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Domain.Common
{
    public class BillingConsts
    {
        public const string KEY_CARD_PAYMENT_REQUEST_PROCESSING_QUEUE = "CARD_PAYMENT_REQUEST_PROCESSING_QUEUE";  // Q0

        public const string KEY_CARD_PAYMENT_REQUEST_RETRY_QUEUE = "CARD_PAYMENT_REQUEST_RETRY_QUEUE";  // Q1

        public const string KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE = "CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE";  // Q2

        public const string KEY_CARD_PAYMENT_CALLBACK_RETRY_QUEUE = "CARD_PAYMENT_CALLBACK_RETRY_QUEUE";  // Q3

        public const string ALIPAY_TRADE_FINISHED = "TRADE_FINISHED";
    }
}
