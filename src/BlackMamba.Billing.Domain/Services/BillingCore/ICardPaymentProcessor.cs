using System;
using BlackMamba.Framework.RedisMapper;
using BlackMamba.Billing.Domain.Services;
using BlackMamba.Billing.Models.Payments;
using BlackMamba.Billing.Models;

namespace BlackMamba.Billing.Domain
{
    public interface ICardPaymentProcessor
    {
        SubSonic.Oracle.Repository.IRepository oracleRepo { get; set; }
        IPaymentsService PaymentsService { get; set; }
        CardPaymentRequestStatus ProcessCardPaymentRequest(CardPayment cardPayment, int retryCount);
        IRedisService RedisService { get; set; }
        IRESTfulClient RESTfullClient { get; set; }
        int RetrySendCardPaymentCallBack(int retryCount);
        int RetrySendCardPaymentRequestToYP(int retryCount);
        void SendCardPaymentCallBack();
        void SendCardPaymentRequestToYP();
    }
}
