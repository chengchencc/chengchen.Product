using System;
using System.Collections.Generic;
using com.yeepay.cmbn;
using BlackMamba.Framework.Core;
using BlackMamba.Framework.RedisMapper;
using SubSonic.Oracle.Repository;
using BlackMamba.Billing.Models;
using BlackMamba.Billing.Models.Payments;
using BlackMamba.Billing.Domain.Common;
using BlackMamba.Billing.Domain.Mappers;
using BlackMamba.Billing.Models.Billing;
using BlackMamba.Billing.Domain.Services;

namespace BlackMamba.Billing.Domain
{
    public class CardPaymentProcessor : ICardPaymentProcessor
    {
        public IMailService PaymentsService { get; set; }
        public IRedisService RedisService { get; set; }
        public IRepository oracleRepo { get; set; }
        public IRESTfulClient RESTfullClient { get; set; }
        int retryTimesLimitation = "RetryTimes".ConfigValue().ToInt32();
        int timeoutMilliSeconds = 3000;

        public CardPaymentProcessor(IMailService paymentsService, IRedisService redisService, IRESTfulClient restfulClient)
        {
            PaymentsService = paymentsService;
            RedisService = redisService;
            oracleRepo = new SimpleRepository(ConnectionStrings.KEY_ORACLE_GENERAL, SimpleRepositoryOptions.RunMigrations);
            RESTfullClient = restfulClient;

            if (retryTimesLimitation == 0) retryTimesLimitation = 2;
        }

        #region Card Payment Request
        public void SendCardPaymentRequestToYP()
        {
            CardPayment cardPayment = RedisService.RetrieveItemFromQueue<CardPayment>(BillingConsts.KEY_CARD_PAYMENT_REQUEST_PROCESSING_QUEUE);
            ProcessCardPaymentRequest(cardPayment, 0);
        }

        public CardPaymentRequestStatus ProcessCardPaymentRequest(CardPayment cardPayment, int retryCount)
        {
            var result = CardPaymentRequestStatus.NotRequest;
            // SZXResult szxResult = PaymentsService.YeepayCardPayments(cardPayment); //卡内金额全部充值
            SZXResult szxResult = PaymentsService.YeepayCardPartialPayments(cardPayment); //部分充值


            LogHelper.WriteInfo(string.Format("Processing card payment with OrderNo : {0}, request result : {1}", cardPayment.OrderNo, szxResult == null ? "null" : szxResult.R1_Code));
            if (szxResult == null || string.IsNullOrEmpty(szxResult.R1_Code)) // retry 
            {
                if (retryCount == -1)
                {
                    result = CardPaymentRequestStatus.RequestFailed;
                }
                else
                {
                    cardPayment.RequestDateTime = DateTime.Now;
                    retryCount++;
                    result = CardPaymentRequestStatus.RequestFailed;

                    if (retryCount <= retryTimesLimitation)
                    {
                        RedisService.AddItemToQueue<CardPayment>(BillingConsts.KEY_CARD_PAYMENT_REQUEST_RETRY_QUEUE + retryCount, cardPayment);
                    }
                    else // final failed
                    {
                        Order currentOrder = oracleRepo.Single<Order>(cardPayment.OrderID);
                        SetFailedOrderStatus(CardPaymentRequestStatus.RequestFailed, currentOrder);
                        //record the info for retry manually
                        var cardPaymentInfoForRetry = EntityMapping.Auto<CardPayment, CardPaymentRetry>(cardPayment);
                        var AddRes = oracleRepo.Add<CardPaymentRetry>(cardPaymentInfoForRetry);
                    }
                }

            }
            else
            {
                CardPaymentRequestStatus requestStatus = (CardPaymentRequestStatus)szxResult.R1_Code.ToInt32();
                Order currentOrder = oracleRepo.Single<Order>(cardPayment.OrderID);

                if (currentOrder != null)
                {
                    switch (requestStatus)
                    {
                        case CardPaymentRequestStatus.Success:
                            SetSuccessOrderStatus(currentOrder);
                            result = requestStatus;
                            break;
                        default:
                            SetFailedOrderStatus(requestStatus, currentOrder);
                            result = requestStatus;
                            break;
                    }
                }
            }
            return result;
        }

        public int RetrySendCardPaymentRequestToYP(int retryCount)
        {
            if (RedisService.GetLengthOfQueue<CardPayment>(BillingConsts.KEY_CARD_PAYMENT_REQUEST_RETRY_QUEUE + retryCount) == 0)
            {
                return 0;
            }
            List<CardPayment> cardPayments = RedisService.GetAllItemsFromQueue<CardPayment>(BillingConsts.KEY_CARD_PAYMENT_REQUEST_RETRY_QUEUE + retryCount);
            var retryIntervals = "RetryIntervals".ConfigValue().Split(',');
            int processedCount = 0;

            foreach (var cardPayment in cardPayments)
            {
                if (retryCount <= retryTimesLimitation)
                {
                    if ((DateTime.Now - cardPayment.RequestDateTime).TotalSeconds >= retryIntervals[retryCount - 1].ToInt32())
                    {
                        LogHelper.WriteInfo(string.Format("Retry sending Card Payment request for OrderNo : {0}, retry count : {1}", cardPayment.OrderNo, retryCount));
                        int removeCount = RedisService.RemoveItemFromQueue(BillingConsts.KEY_CARD_PAYMENT_REQUEST_RETRY_QUEUE + retryCount, cardPayment);
                        ProcessCardPaymentRequest(cardPayment, retryCount);
                        processedCount++;
                    }
                }
            }

            return processedCount;
        }

        private void SetFailedOrderStatus(CardPaymentRequestStatus resultCode, Order currentOrder)
        {
            currentOrder.OrderStatus = OrderStatus.Failed;
            currentOrder.CardPaymentRequestStatus = resultCode;
            int updateRet = oracleRepo.Update<Order>(currentOrder);

            if (!string.IsNullOrEmpty(currentOrder.CallBackUrl))
            {
                RedisService.AddItemToQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE,
                    new PaymentNotification
                    {
                        ResultCode = (int)CardPaymentCallBackStatus.RequestError,
                        OrderNo = currentOrder.OrderNo,
                        Description = CardPaymentDataDict.CardPaymentRequestStatusDesc[((int)resultCode).ToString()],
                        RequestAmount = currentOrder.Amount,
                        OutOrderNo = currentOrder.OutOrderNo,
                        SuccessAmount = 0.0f,
                        CallbackURL = currentOrder.CallBackUrl,
                        LastRequestDate = DateTime.Now
                    }
                );
            }

        }

        private void SetSuccessOrderStatus(Order currentOrder)
        {
            currentOrder.OrderStatus = OrderStatus.Processing;
            currentOrder.CardPaymentRequestStatus = CardPaymentRequestStatus.Success;
            int updateRet = oracleRepo.Update<Order>(currentOrder);
        }
        #endregion

        #region Card payment Call back

        public void SendCardPaymentCallBack()
        {
            PaymentNotification callbackInfo = RedisService.RetrieveItemFromQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE);
            ProcessCallbackRequest(callbackInfo, 0);
        }

        public int RetrySendCardPaymentCallBack(int retryCount)
        {
            if (RedisService.GetLengthOfQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_RETRY_QUEUE + retryCount) == 0)
            {
                return 0;
            }
            List<PaymentNotification> callbackInfos = RedisService.GetAllItemsFromQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_RETRY_QUEUE + retryCount);
            var retryIntervals = "RetryIntervals".ConfigValue().Split(',');
            int processedCount = 0;
            foreach (var callbackInfo in callbackInfos)
            {
                if (retryCount <= retryTimesLimitation)
                {
                    if ((DateTime.Now - callbackInfo.LastRequestDate).TotalSeconds >= retryIntervals[retryCount - 1].ToInt32())
                    {
                        LogHelper.WriteInfo(string.Format("Retry sending Card Payment CallBack for OrderNo : {0}, retry count : {1}", callbackInfo.OrderNo, retryCount), ConsoleColor.Green);
                        int removeCount = RedisService.RemoveItemFromQueue(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_RETRY_QUEUE + retryCount, callbackInfo);
                        ProcessCallbackRequest(callbackInfo, retryCount);
                        processedCount++;

                    }
                }

            }
            return processedCount;
        }

        internal void ProcessCallbackRequest(PaymentNotification callbackInfo, int retryCount)
        {
            if (string.IsNullOrEmpty(callbackInfo.CallbackURL))
            {
                var currentOrder = oracleRepo.Single<Order>(s => s.OrderNo == callbackInfo.OrderNo);
                if (currentOrder != null)
                {
                    FailedNofity(callbackInfo, currentOrder);
                }

                return;
            }

            // 有了用的 var url = string.Format("{0}?orderno={1}&resultcode={2}&requestamount={3}&successamount={4}&desc={5}", callbackInfo.CallbackURL, callbackInfo.OrderNo, callbackInfo.ResultCode.ToString(), callbackInfo.RequestAmount.ToString(), callbackInfo.SuccessAmount.ToString(), callbackInfo.Description);
            var url = string.Format("{0}?orderId={1}&status={2}&globalId={3}", callbackInfo.CallbackURL,callbackInfo.OutOrderNo, callbackInfo.ResultCode.ToString(), callbackInfo.OrderNo);
            
            var response = RESTfullClient.Get(url, timeoutMilliSeconds);
            LogHelper.WriteInfo(string.Format("Send call back to client, order no : {0}, request result : {1}", callbackInfo.OrderNo, response), ConsoleColor.Green);

            if (!string.IsNullOrEmpty(response) && response.StartsWith("success", StringComparison.OrdinalIgnoreCase))
            {
                var currentOrder = oracleRepo.Single<Order>(s => s.OrderNo == callbackInfo.OrderNo);
                if (currentOrder != null)
                {
                    if (currentOrder.OrderStatus == OrderStatus.Processing)
                    {
                        //processing status without operation
                    }
                    else
                    {
                        SuccessNofity(callbackInfo, currentOrder);
                    }
                }
            }
            else
            {
                RetryNotify(callbackInfo, retryCount);
            }

        }

        private int RetryNotify(PaymentNotification callbackInfo, int retryCount)
        {
            callbackInfo.LastRequestDate = DateTime.Now;
            retryCount++;
            if (retryCount <= retryTimesLimitation)
            {
                RedisService.AddItemToQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_RETRY_QUEUE + retryCount, callbackInfo);
            }
            else// final failed
            {
                Order currentOrder = oracleRepo.Single<Order>(s => s.OrderNo == callbackInfo.OrderNo);
                if (currentOrder.OrderStatus == OrderStatus.Processing)
                {
                    //processing status without operation
                }
                else
                {
                    FailedNofity(callbackInfo, currentOrder);
                }
            }
            return retryCount;
        }

        private void SuccessNofity(PaymentNotification callbackInfo, Order currentOrder)
        {
            currentOrder.OrderStatus = OrderStatus.Complete;
            var updateRet = oracleRepo.Update<Order>(currentOrder);
            //成功    从通知队列中把通知model删除 记录成功信息
            var callbackNotifyDetails = EntityMapping.Auto<PaymentNotification, CallbackNotification>(callbackInfo);
            callbackNotifyDetails.IsNotifySuccess = true;
            callbackNotifyDetails.LastRequestDate = DateTime.Now;
            var addRet = oracleRepo.Add<CallbackNotification>(callbackNotifyDetails);
        }

        private void FailedNofity(PaymentNotification callbackInfo, Order currentOrder)
        {
            currentOrder.OrderStatus = OrderStatus.Stoped;
            var updateRet = oracleRepo.Update<Order>(currentOrder);

            var callbackNotifyDetails = EntityMapping.Auto<PaymentNotification, CallbackNotification>(callbackInfo);
            callbackNotifyDetails.IsNotifySuccess = false;
            callbackNotifyDetails.LastRequestDate = DateTime.Now;
            var addRet = oracleRepo.Add<CallbackNotification>(callbackNotifyDetails);
        }


        #endregion

    }
}
