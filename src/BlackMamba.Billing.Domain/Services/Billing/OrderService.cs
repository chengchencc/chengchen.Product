using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SubSonicOracle = SubSonic.Oracle.Repository;
using NFunPay.Models.Payments;
using SubSonic.Oracle.Repository;


namespace NFunPay.Domain.Services
{
    public class OrderService : IOrderService
    {
        public IRepository SimpleRepository { get; set; }
        public SubSonicOracle.IRepository SimpleOracleRepository { get; set; }
        private object sync = new object();
        const string ORDERCOUNT = "SYSTEM:ORDERCOUNT";
        const string GAMECENTER_PRODUCT_NAME = "网游中心充值";
       // public ICacheManagerHelper CacheManagerHelper { get; set; }

        public OrderService(ICacheManagerHelper cacheManagerHelper)
        {
            this.SimpleRepository = new SimpleRepository(ConnectionStrings.KEY_ORDER, SimpleRepositoryOptions.RunMigrations);
            this.SimpleOracleRepository = new SubSonicOracle.SimpleRepository(ConnectionStrings.KEY_ORACLE_GENERAL, SubSonicOracle.SimpleRepositoryOptions.RunMigrations);
            this.CacheManagerHelper = cacheManagerHelper;
        }

        #region original order
        public string Order(string imsi, string targetContentId, string productCode, string orderNo, float price = 0)
        {
            if (price <= 0)
            {
                return null;
            }
            Order order = new Order();
            order.Imsi = imsi;
            order.ProductCode = productCode;
            order.Status = OrderStatus.Created;
            order.TargetContentID = targetContentId;
            order.CreateDate = DateTime.Now;
            order.Price = price;
            try
            {
                order.OrderNo = orderNo;
                SimpleRepository.Add<Order>(order);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ErrorLogger").Error(string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
                return null;
            }
            return order.OrderNo;
        }

        public IList<Order> SearchOrder(string imsi, List<string> targetContentId, string productCode)
        {
            var res = SimpleRepository.Find<Order>(s => s.Imsi == imsi && s.Status == OrderStatus.Successed && s.ProductCode == productCode && targetContentId.ToArray().Contains(s.TargetContentID));
            return res;
        }

        public string GenerateOrderNumber()
        {
            var orderCount = 0;
            int maxCount = 10000000;
            lock (this.sync)
            {
                orderCount = (int)(CacheManagerHelper.Increment(ORDERCOUNT) % maxCount);
                return string.Format("{0}{1}", DateTime.Now.ToString(DateTimeFormat.yyyyMMddHHmmss), orderCount.ToString().PadLeft(7, '0'));
            }
        }

        public string UpdateStatus(string orderNo, int status)
        {
            var order = SimpleRepository.Single<Order>(s => s.OrderNo == orderNo);
            if (order != null)
            {
                switch (status)
                {
                    case (int)OrderStatus.Created:
                        order.Status = OrderStatus.Created;
                        break;
                    case (int)OrderStatus.Successed:
                        order.Status = OrderStatus.Successed;
                        order.PurchaseDate = DateTime.Now;
                        break;
                    case (int)OrderStatus.Failed:
                        order.Status = OrderStatus.Failed;
                        order.PurchaseDate = DateTime.Now;
                        break;
                    case (int)OrderStatus.Canceled:
                        order.Status = OrderStatus.Canceled;
                        break;
                    case (int)OrderStatus.Expired:
                        order.Status = OrderStatus.Expired;
                        break;
                    case (int)OrderStatus.Processing:
                        order.Status = OrderStatus.Processing;
                        break;
                    case (int)OrderStatus.Stoped:
                        order.Status = OrderStatus.Stoped;
                        break;
                    default:
                        break;
                }

                SimpleRepository.Update<Order>(order);
            }

            return orderNo;
        }
        #endregion

        #region customer order

        public string GenerateCustomerOrder(int customerId, float amount, string productName = GAMECENTER_PRODUCT_NAME)
        {
            if (amount <= 0)
            {
                return null;
            }
            CustomerOrder customerOrder = new CustomerOrder();
            customerOrder.CustomerId = customerId;
            customerOrder.Amount = amount;
            customerOrder.CreateDate = DateTime.Now;
            customerOrder.Status = OrderStatus.Created;
            customerOrder.ProductName = productName;
            try
            {
                customerOrder.OrderNo = GenerateOrderNumber();
                SimpleOracleRepository.Add<CustomerOrder>(customerOrder);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ErrorLogger").Error(string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
                return null;
            }
            return customerOrder.OrderNo;
        }

        public CustomerOrder GetCustomerOrder(string orderNo)
        {
            if (string.IsNullOrEmpty(orderNo))
            {
                return null;
            }
            var customerOrder = SimpleOracleRepository.Single<CustomerOrder>(s=>s.OrderNo==orderNo);            
            return customerOrder;
        }

        #endregion
    }
}
