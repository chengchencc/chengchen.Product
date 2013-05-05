using System.Collections.Generic;
using BlackMamba.Framework.RedisMapper;
using SubSonic.Oracle.Repository;
using SubSonic.Oracle.Schema;
using BlackMamba.Billing.Models.Payments;
using System.Linq;
using BlackMamba.Billing.Models;
using BlackMamba.Billing.Domain.Mappers;
using BlackMamba.Billing.Domain.Common;


namespace BlackMamba.Billing.Domain.Services
{
    public class PaymentsUIService : IPaymentsUIService
    {
        protected const int PAGESIZE = 20;

        public IRepository Repository { get; set; }

        public IRedisService RedisService;

        public ICardPaymentProcessor CardPaymentProcessor;

        public PaymentsUIService(IRedisService redisService,ICardPaymentProcessor cardPaymentProcessor)
        {
            this.Repository = new SimpleRepository(ConnectionStrings.KEY_ORACLE_GENERAL, SimpleRepositoryOptions.RunMigrations);

            CardPaymentProcessor = cardPaymentProcessor;
            this.RedisService = redisService;
        }

        public PagedList<Order> GetAllOrders(int pageIndex, int pageSize)
        {
            var orderList = Repository.GetPaged<Order>(pageIndex, pageSize,x=>x.CreatedDate,true);
            //Repository;

            return orderList;
        }

        public Order Search(string orderNo, int orderStatu)
        {
            if (orderStatu==404) {  return string.IsNullOrEmpty(orderNo) ? null : Repository.Single<Order>(x => x.OrderNo == orderNo); }
            return string.IsNullOrEmpty(orderNo) ? null : Repository.Single<Order>(s=>s.OrderNo == orderNo && s.OrderStatus == (Models.Payments.OrderStatus) orderStatu); 
        }

        public List<Order> SearchOrders(string userId, int OrderStatus)
        {
            if (OrderStatus != 404 && !string.IsNullOrEmpty(userId))
            {
                return Repository.Find<Order>(x => x.UserId == userId && x.OrderStatus == (OrderStatus)OrderStatus).ToList();
            }

            if (OrderStatus != 404)
            {
                return Repository.Find<Order>(x => x.OrderStatus == (OrderStatus)OrderStatus).ToList();
            }

            if (!string.IsNullOrEmpty(userId))
            { 
                return Repository.Find<Order>(x => x.UserId == userId).ToList();
            }

            return new List<Order>();
        }

        public PagedList<PaymentNotification> CallBackProcessingDataList(int pageNum, int pageSize, string OrderNo = "")
        {
            var paymentNotificationList = this.RedisService.GetAllItemsFromQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE);

            var pageList = new PagedList<PaymentNotification>(paymentNotificationList.Skip((pageNum - 1) * pageSize).Take(pageSize), paymentNotificationList.Count, pageNum, pageSize);
            
            if (!string.IsNullOrEmpty(OrderNo)&&paymentNotificationList.Count>0) {
                var paymentNotification=  paymentNotificationList.Find(x => x.OrderNo == OrderNo);
                paymentNotificationList.Clear();
                if (paymentNotification != null)
                {
                    paymentNotificationList.Add(paymentNotification);
                }
                pageList = new PagedList<PaymentNotification>(paymentNotificationList, paymentNotificationList.Count, 1, pageSize);
               
            }
            return pageList;
        }

        public PagedList<Order> GetFailOrder(int pageNum)
        {
            return this.Repository.GetPaged<Order>(pageNum, PAGESIZE, (s => s.OrderStatus == OrderStatus.Failed), s => s.CreatedDate,true);
        }

        public PagedList<Order> GetProccessedOrder(int pageIndex, int pageSize)
        {
            return Repository.GetPaged<Order>(pageIndex, pageSize, (s => s.OrderStatus == OrderStatus.Complete), s => s.CreatedDate,true);
        }

        public PagedList<Order> GetPaymentingOrder(int pageNum)
        {
            return this.Repository.GetPaged<Order>(pageNum, PAGESIZE, (s => s.OrderStatus == OrderStatus.Processing), s => s.CreatedDate,true);
        }

        public PagedList<CardPaymentRetry> GetAllRetryPanyments(int pageNum)
        {
            return Repository.GetPaged<CardPaymentRetry>(pageNum-1 , PAGESIZE,x=>x.CreatedDate,true);
        }

        public PagedList<CardPaymentRetry> SearchedByCardNo(string keyword,int pageNum)
        {
            return Repository.GetPaged<CardPaymentRetry>(pageNum-1, PAGESIZE, (s => s.CardNo.Contains(keyword)), s => s.CreatedDate,true);

        }

        public CardPaymentRequestStatus RetryRequest(string id)
        {
            var cardPaymentRetry = Repository.Single<CardPaymentRetry>(id);

            var cp = EntityMapping.Auto<CardPaymentRetry, CardPayment>(cardPaymentRetry);
            var res = CardPaymentProcessor.ProcessCardPaymentRequest(cp, -1);
            switch (res)
            {
                case CardPaymentRequestStatus.RequestFailed:
                    break;
                case CardPaymentRequestStatus.NotRequest:
                    break;
                default:
                    Repository.Delete<CardPaymentRetry>(id);
                    break;
            }
            return res;
        }

        public List<OrderLine> GetOrderLine(string orderNo)
        {
            return Repository.Find<OrderLine>(s => s.OrderNo == orderNo).ToList();
        }

    }
}
