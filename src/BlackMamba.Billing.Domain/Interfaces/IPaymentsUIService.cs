using System.Collections.Generic;
using SubSonic.Oracle.Schema;
using BlackMamba.Billing.Models.Payments;
using BlackMamba.Billing.Models;

namespace BlackMamba.Billing.Domain.Services
{
   public interface IPaymentsUIService
    {
       PagedList<Order> GetAllOrders(int pageIndex, int pageSize);
       Order Search(string orderNo,int orderStatu);
       List<Order> SearchOrders(string userId, int OrderStatus);

       PagedList<PaymentNotification> CallBackProcessingDataList(int pageNum, int pageSize,string OrderNo);
       PagedList<Order> GetProccessedOrder(int pageIndex, int pageSize);
       PagedList<CardPaymentRetry> GetAllRetryPanyments(int pageNum);

       PagedList<Order> GetFailOrder(int pageNum);
       PagedList<Order> GetPaymentingOrder(int pageNum);
       CardPaymentRequestStatus RetryRequest(string id);
       PagedList<CardPaymentRetry> SearchedByCardNo(string keyword, int pageNum);
       List<OrderLine> GetOrderLine(string orderNo);
    }
}
