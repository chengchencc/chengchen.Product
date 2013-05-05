using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SubSonic.Oracle.Repository;
using Xunit;
using BlackMamba.Framework.RedisMapper;
using Moq;
using BlackMamba.Billing.Domain.Services;
using BlackMamba.Billing.Domain;
using BlackMamba.Billing.Models;
using BlackMamba.Billing.Models.Payments;
using com.yeepay.cmbn;
using BlackMamba.Billing.Domain.Common;

namespace BlackMamba.Billing.Tests
{
    public class CardPaymentProcessorTest
    {
        Mock<IRedisService> _redisServiceMock;
        IRedisService redisService;

        Mock<IPaymentsService> _paymentsServiceMock;
        IPaymentsService paymentsService;

        IRepository mockRepository;
        Mock<IRepository> _repository;

        Mock<IRESTfulClient> _restfulClientMock;
        IRESTfulClient restfulClient;

        CardPaymentProcessor cardPaymentProcessor;

        CardPayment cp = new CardPayment
        {
            OrderID = 1,
            OrderNo = "111",
        };

        PaymentNotification callbackInfo = new PaymentNotification()
        {
            CallbackURL = @"http://www.youle.com/",
            RequestAmount = 100f,
            LastRequestDate = DateTime.Now,
            Description = "Desc",
            OrderNo = "20071110558487745",
            OutOrderNo="12211221",
            ResultCode = 1,
            SuccessAmount = 100f
        };


        Order currentOrder = new Order();

        public CardPaymentProcessorTest()
        {
            _redisServiceMock = new Mock<IRedisService>();
            redisService = _redisServiceMock.Object;

            _repository = new Mock<IRepository>();
            mockRepository = _repository.Object;

            _paymentsServiceMock = new Mock<IPaymentsService>();
            paymentsService = _paymentsServiceMock.Object;

            _restfulClientMock = new Mock<IRESTfulClient>();
            restfulClient = _restfulClientMock.Object;

            cardPaymentProcessor = new CardPaymentProcessor(paymentsService, redisService, restfulClient);
            cardPaymentProcessor.oracleRepo = mockRepository;

            cp.RequestDateTime = DateTime.Now;
            currentOrder.Id = (int)cp.OrderID;
            Bootstrapper.Start();


        }

        [Fact]
        public void add_card_payment_request_to_retry_queue_when_yeepaycardpayments_return_null()
        {
            _paymentsServiceMock.Setup<SZXResult>(s => s.YeepayCardPayments(cp)).Returns(default(SZXResult));
            _redisServiceMock.Setup(s => s.RetrieveItemFromQueue<CardPayment>(BillingConsts.KEY_CARD_PAYMENT_REQUEST_PROCESSING_QUEUE)).Returns(cp);

            cardPaymentProcessor.SendCardPaymentRequestToYP();

            // add it into first retry queue
            _redisServiceMock.Verify(s => s.AddItemToQueue<CardPayment>(BillingConsts.KEY_CARD_PAYMENT_REQUEST_RETRY_QUEUE + 1, cp));
        }

        [Fact]
        public void update_order_status_when_yeepaycardpayments_return_normal_result()
        {
            SZXResult result = new SZXResult("", "1", "", "", "", "", "");
            Order currentOrder = new Order
            {
                Id = (int)cp.OrderID,
            };

            _paymentsServiceMock.Setup<SZXResult>(s => s.YeepayCardPartialPayments(cp)).Returns(result);
            _repository.Setup(s => s.Single<Order>(cp.OrderID)).Returns(currentOrder);

            cardPaymentProcessor.ProcessCardPaymentRequest(cp, 0);

            _repository.Verify(s => s.Update<Order>(currentOrder));
        }

        [Fact]
        public void update_order_status_and_add_to_CardPaymentCallbackResult_processing_queue_when_yeepaycardpayments_return_abnormal_result()
        {
            SZXResult result = new SZXResult("", "-1", "", "", "", "", "");


            _paymentsServiceMock.Setup<SZXResult>(s => s.YeepayCardPartialPayments(cp)).Returns(result);
            _repository.Setup(s => s.Single<Order>(cp.OrderID)).Returns(currentOrder);

            cardPaymentProcessor.ProcessCardPaymentRequest(cp, 0);

            // should update order status
            _repository.Verify(s => s.Update<Order>(currentOrder));
            // should put into Q2
            //_redisServiceMock.Verify(s => s.AddItemToQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE, It.IsAny<PaymentNotification>()));
        }

        [Fact]
        public void first_retry_should_be_performed_if_retry_times_not_rearched_and_retry_interval_rearched_or_exceeded()
        {
            _redisServiceMock.Setup(s => s.GetLengthOfQueue<CardPayment>(BillingConsts.KEY_CARD_PAYMENT_REQUEST_RETRY_QUEUE + 1)).Returns(1);
            _redisServiceMock.Setup(s => s.GetAllItemsFromQueue<CardPayment>(BillingConsts.KEY_CARD_PAYMENT_REQUEST_RETRY_QUEUE + 1)).
                Returns(new List<CardPayment> { cp });
            _paymentsServiceMock.Setup<SZXResult>(s => s.YeepayCardPayments(cp)).Returns(default(SZXResult));

            var processedCount = cardPaymentProcessor.RetrySendCardPaymentRequestToYP(1);

            Assert.Equal(1, processedCount);

            // if it failed again, should add it into the second retry queue
            _redisServiceMock.Verify(s => s.AddItemToQueue<CardPayment>(BillingConsts.KEY_CARD_PAYMENT_REQUEST_RETRY_QUEUE + 2, cp));
            // remove from the first retry queue
            _redisServiceMock.Verify(s => s.RemoveItemFromQueue<CardPayment>(BillingConsts.KEY_CARD_PAYMENT_REQUEST_RETRY_QUEUE + 1, cp));
        }

        [Fact]
        public void second_retry_should_not_be_performed_if_retry_times_not_rearched_and_retry_interval_not_rearched_or_exceeded()
        {
            _redisServiceMock.Setup(s => s.GetAllItemsFromQueue<CardPayment>(BillingConsts.KEY_CARD_PAYMENT_REQUEST_RETRY_QUEUE + 2)).
                Returns(new List<CardPayment> { cp });

            var processedCount = cardPaymentProcessor.RetrySendCardPaymentRequestToYP(2);

            // if execute time not rearched, nothing happened, still in original queue
            Assert.Equal(0, processedCount);
        }

        [Fact]
        public void after_last_retry_should_set_failed_order_status()
        {
            _redisServiceMock.Setup(s => s.GetLengthOfQueue<CardPayment>(BillingConsts.KEY_CARD_PAYMENT_REQUEST_RETRY_QUEUE + 5)).Returns(1);
            _redisServiceMock.Setup(s => s.GetAllItemsFromQueue<CardPayment>(BillingConsts.KEY_CARD_PAYMENT_REQUEST_RETRY_QUEUE + 5)).
                Returns(new List<CardPayment> { cp });
            _paymentsServiceMock.Setup<SZXResult>(s => s.YeepayCardPayments(cp)).Returns(default(SZXResult));
            _repository.Setup(s => s.Single<Order>(cp.OrderID)).Returns(currentOrder);

            var processedCount = cardPaymentProcessor.RetrySendCardPaymentRequestToYP(5);

            Assert.Equal(1, processedCount);

            // last retry failed, should Set Failed Order Status
            // should update order status
            _repository.Verify(s => s.Update<Order>(currentOrder));
            // should put into Q2
            //_redisServiceMock.Verify(s => s.AddItemToQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE, It.IsAny<PaymentNotification>()));
            // remove from the last retry queue
            _redisServiceMock.Verify(s => s.RemoveItemFromQueue<CardPayment>(BillingConsts.KEY_CARD_PAYMENT_REQUEST_RETRY_QUEUE + 5, cp));
            _repository.Verify(s => s.Add<CardPaymentRetry>(It.IsAny<CardPaymentRetry>()));

        }

        [Fact]
        public void add_card_payment_notification_to_retry_queue_when_response_is_null()
        {

            _redisServiceMock.Setup(s => s.RetrieveItemFromQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE)).Returns(callbackInfo);
            _restfulClientMock.Setup(s => s.Get("http://www.youle.com/?orderno=20071110558487745&resultcode=1&requestamount=100&successamount=100&desc=Desc", 3000)).Returns(string.Empty);

            cardPaymentProcessor.SendCardPaymentCallBack();
            _redisServiceMock.Verify(s => s.AddItemToQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_RETRY_QUEUE + 1, callbackInfo), Times.Once());
            _repository.Verify(s => s.Single<Order>(It.IsAny<Expression<Func<Order, bool>>>()), Times.Never());
        }

        [Fact]
        public void add_card_payment_notification_to_retry_queue_when_response_is_not_success()
        {
            var errorResponse = "abaaa";
            _redisServiceMock.Setup(s => s.RetrieveItemFromQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE)).Returns(callbackInfo);
            _restfulClientMock.Setup(s => s.Get("http://www.youle.com/?orderno=20071110558487745&resultcode=1&requestamount=100&successamount=100&desc=Desc", 3000)).Returns(errorResponse);

            cardPaymentProcessor.SendCardPaymentCallBack();
            _redisServiceMock.Verify(s => s.AddItemToQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_RETRY_QUEUE + 1, callbackInfo), Times.Once());
            _repository.Verify(s => s.Single<Order>(It.IsAny<Expression<Func<Order, bool>>>()), Times.Never());
        }

        [Fact]
        public void add_card_payment_notification_to_retry_queue_when_response_is_success()
        {
            var sccessResponse = "Success";
            Order order = new Order()
            {
                Id = 1,
                Amount = 100,
                CardPaymentRequestStatus = CardPaymentRequestStatus.Success,
                CardPaymentCallBackStatus = CardPaymentCallBackStatus.Success,
                Currency = "CNY",
                OrderNo = "20071110558487745",
                OrderStatus = OrderStatus.Successed,
                PayedAmount = 100,
            };

            _redisServiceMock.Setup(s => s.RetrieveItemFromQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE)).Returns(callbackInfo);
            _restfulClientMock.Setup(s => s.Get("http://www.youle.com/?orderId=12211221&status=1&globalId=20071110558487745", 3000)).Returns(sccessResponse);
            _repository.Setup(s => s.Single<Order>(It.IsAny<Expression<Func<Order, bool>>>())).Returns(order);

            cardPaymentProcessor.SendCardPaymentCallBack();

            _redisServiceMock.Verify(s => s.AddItemToQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_RETRY_QUEUE, It.IsAny<PaymentNotification>()), Times.Never());
            _repository.Verify(s => s.Single<Order>(It.IsAny<Expression<Func<Order, bool>>>()), Times.Once());
            _repository.Verify(s => s.Update<Order>(order), Times.Once());
            _repository.Verify(s => s.Add<CallbackNotification>(It.IsAny<CallbackNotification>()), Times.Once());

        }

        [Fact]
        public void first_notify_retry_should_be_performed_if_retry_times_not_rearched_and_retry_interval_rearched_or_exceeded()
        {
            _redisServiceMock.Setup(s => s.GetLengthOfQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_RETRY_QUEUE + 1)).Returns(1);
            _redisServiceMock.Setup(s => s.GetAllItemsFromQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_RETRY_QUEUE + 1)).Returns(new List<PaymentNotification> { callbackInfo });
            _restfulClientMock.Setup(s => s.Get("http://www.youle.com/?orderno=20071110558487745&resultcode=1&requestamount=100&successamount=100&desc=Desc", 3000)).Returns(string.Empty);

            var process = cardPaymentProcessor.RetrySendCardPaymentCallBack(1);
            Assert.Equal(process, 1);

            _redisServiceMock.Verify(s => s.RemoveItemFromQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_RETRY_QUEUE + 1, callbackInfo));

            _redisServiceMock.Verify(s => s.AddItemToQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_RETRY_QUEUE + 2, callbackInfo));
        }

        [Fact]
        public void second_notify_retry_should_not_be_performed_if_retry_times_not_rearched_and_retry_interval_not_rearched_or_exceeded()
        {
            _redisServiceMock.Setup(s => s.GetAllItemsFromQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_RETRY_QUEUE + 2)).Returns(new List<PaymentNotification> { callbackInfo });
            _restfulClientMock.Setup(s => s.Get("http://www.youle.com/?orderno=20071110558487745&resultcode=1&requestamount=100&successamount=100&desc=Desc", 3000)).Returns(string.Empty);

            var process = cardPaymentProcessor.RetrySendCardPaymentCallBack(2);
            Assert.Equal(process, 0);
        }

        [Fact]
        public void after_last_notify_retry_should_set_failed_order_status()
        {
            Order order = new Order()
            {
                Id = 1,
                Amount = 100,
                CardPaymentRequestStatus = CardPaymentRequestStatus.Success,
                CardPaymentCallBackStatus = CardPaymentCallBackStatus.Success,
                Currency = "CNY",
                OrderNo = "20071110558487745",
                OrderStatus = OrderStatus.Successed,
                PayedAmount = 100,
            };
            _redisServiceMock.Setup(s => s.GetLengthOfQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_RETRY_QUEUE + 5)).Returns(1);
            _redisServiceMock.Setup(s => s.GetAllItemsFromQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_RETRY_QUEUE + 5)).Returns(new List<PaymentNotification> { callbackInfo });
            _restfulClientMock.Setup(s => s.Get("http://www.youle.com/?orderno=20071110558487745&resultcode=1&requestamount=100&successamount=100&desc=Desc", 3000)).Returns(string.Empty);
            _repository.Setup(s => s.Single<Order>(It.IsAny<Expression<Func<Order, bool>>>())).Returns(order);

            var process = cardPaymentProcessor.RetrySendCardPaymentCallBack(5);

            Assert.Equal(1, process);
            _repository.Verify(s => s.Update<Order>(order));
            _repository.Verify(s => s.Add<CallbackNotification>(It.IsAny<CallbackNotification>()), Times.Once());
        }

        [Fact]
        public void card_payment_process_retry_param_test()
        {
            CardPayment cp = new CardPayment()
            {
                OrderNo = "200111201200012",
                ProductName = "网游棋牌",
                CardNo = "32554112556544231588",
                CardPassword = "4554478845",
                Amount = 100,
                OrderID = 2,
                VerifyAmount = false,
                CardAmount = 100,
            };

            _paymentsServiceMock.Setup<SZXResult>(s => s.YeepayCardPayments(cp)).Returns(default(SZXResult));

            var res = cardPaymentProcessor.ProcessCardPaymentRequest(cp, -1);
            Assert.Equal(CardPaymentRequestStatus.RequestFailed, res);
            _redisServiceMock.Verify(s => s.AddItemToQueue<CardPayment>(BillingConsts.KEY_CARD_PAYMENT_REQUEST_RETRY_QUEUE + (-1), cp), Times.Never());
            _repository.Verify(s => s.Single<Order>(It.IsAny<Expression<Func<Order, bool>>>()), Times.Never());
        }

        [Fact]
        public void do_not_notify_if_callback_url_is_null()
        {
            var callbackIn = new PaymentNotification()
            {
                CallbackURL = string.Empty,
                OrderNo = "20071110558487745",
            };
            Order order = new Order()
            {
                Id = 1,
                Amount = 100,
                CardPaymentRequestStatus = CardPaymentRequestStatus.Success,
                CardPaymentCallBackStatus = CardPaymentCallBackStatus.Success,
                Currency = "CNY",
                OrderNo = "20071110558487745",
                OrderStatus = OrderStatus.Successed,
                PayedAmount = 100,
            };
            _repository.Setup(s => s.Single<Order>(It.IsAny<Expression<Func<Order, bool>>>())).Returns(order);

            cardPaymentProcessor.ProcessCallbackRequest(callbackIn, 1);
            _repository.Verify(s => s.Update<Order>(It.IsAny<Order>()));
        }
    }
}
