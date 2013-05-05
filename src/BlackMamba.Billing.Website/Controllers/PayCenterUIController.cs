using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Order= BlackMamba.Billing.Models.Payments.Order;
using SubSonic.Oracle.Schema;
using BlackMamba.Billing.Models.Payments;
using BlackMamba.Billing.Domain.Services;
using BlackMamba.Billing.Website.Controllers.Base;


namespace BlackMamba.Billing.Website.Controllers
{
    public class PayCenterUIController : UIBaseController
    {
        #region Ctor

        IPaymentsUIService _paymentUISvc;
        protected const int PAGESIZE = 20;

        public PayCenterUIController(IPaymentsUIService paymentUISvc)
        {
            _paymentUISvc = paymentUISvc;
        }
        #endregion

        // GET: /PayCenter/
        public ActionResult OrderManage(int? page)
        {
            ViewData["OrderStatusTextMapping"] = GetOrderStatusTextMapping();
            int pageNum = page.HasValue ? page.Value : 1;
            var returnList = _paymentUISvc.GetAllOrders(pageNum - 1, PAGESIZE);
            returnList.PageIndex = pageNum;

            if (string.IsNullOrEmpty(ViewBag.Title))
            {
                ViewBag.Title = "订单查询"; //default is the first tab text
            }
            TempData["ParentTitle"] = "订单查询";

            var orderStatusTypes = from OrderStatus o in Enum.GetValues(typeof(OrderStatus))
               select new { Value = (int)o, Text = o.ToString() };

            var orderStatusList = new SelectList(orderStatusTypes, "Value", "Text").ToList();
            orderStatusList.Insert(0, (new SelectListItem { Value = "404", Text = "All" }));

            ViewData["OrderStatus"] = orderStatusList;
            return View(returnList);
        }

        public ActionResult OrderSearchResult(int page = 1, int orderStatu = 404, string title="")
        {
            ViewData["OrderStatusTextMapping"] = GetOrderStatusTextMapping();

            var orderNo = Request.QueryString["textfield"];
            ViewData["Statu"] =orderStatu;
            ViewBag.Title = title;

            var userId = Request.QueryString["userId"];

            List<Order> orderlist = new List<Order>();

            if (!string.IsNullOrEmpty(orderNo))
            {
                var order = _paymentUISvc.Search(orderNo, orderStatu);

                if (order != null)
                {
                    orderlist.Add(order);
                }
            }
            else 
            {
                orderlist = _paymentUISvc.SearchOrders(userId, orderStatu);
            }

            var orderStatusTypes = from OrderStatus o in Enum.GetValues(typeof(OrderStatus))
                                   select new { Value = (int)o, Text = o.ToString() };
            
            var orderStatusList = new SelectList(orderStatusTypes, "Value", "Text", orderStatu).ToList();
            orderStatusList.Insert(0, (new SelectListItem { Value = "404", Text = "All" }));

            ViewData["OrderStatus"] = orderStatusList;

            PagedList<Order> resultList = new PagedList<Order>(orderlist,orderlist.Count,(page - 1) * PAGESIZE, PAGESIZE);

            return View("OrderManage", resultList);
        }

        [HttpGet]
        public ActionResult CallBackProcessingDataList(string orderNo,int page =1)
        {
            var dataList = this._paymentUISvc.CallBackProcessingDataList(page,PAGESIZE,orderNo);
            ViewData["searchKey"] = orderNo;
          
            return View(dataList);
        }

        public ActionResult ProcessedOrder(int? page)
        {
            int pageNum = page.HasValue ? page.Value : 1;
            ViewData["OrderStatusTextMapping"] = GetOrderStatusTextMapping();

            var processedOrders = _paymentUISvc.GetProccessedOrder(pageNum-1,PAGESIZE);
            processedOrders.PageIndex = pageNum;
            ViewBag.Title = "处理完毕订单";
            TempData["ParentTitle"] = "处理完毕订单";

            ViewData["Statu"] = (int)OrderStatus.Complete;
            return View("OrderManage", processedOrders);
        }

        public ActionResult FailureOrder(int? page)
        {
            ViewData["OrderStatusTextMapping"] = GetOrderStatusTextMapping();

            int pageNum = page.HasValue ? page.Value : 1;
            PagedList<BlackMamba.Billing.Models.Payments.Order> orders = this._paymentUISvc.GetFailOrder(pageNum-1);
            orders.PageIndex = pageNum;

            ViewBag.Title = "失败订单";
            TempData["ParentTitle"] = "失败订单";
            ViewData["Statu"] = (int)OrderStatus.Failed;
            return View("OrderManage", orders);
        }

        public ActionResult RetryOrder(int page = 1)
        {
            var orders = this._paymentUISvc.GetAllRetryPanyments(page);
            orders.PageIndex = page;

            ViewBag.Title = "可重试订单";
            return View(orders);
        }

        public ActionResult RetryOrderSearchedByCardNo(string textfield, int page = 1)
        {
            var orders = _paymentUISvc.SearchedByCardNo(textfield, page);
            orders.PageIndex = page;
            return View("RetryOrder", orders);
        }

        public ActionResult RetryRequest(string id)
        {
            var res = _paymentUISvc.RetryRequest(id);
            return Content(res.ToString());
        }

        public ActionResult LackOfPermission()
        {
            return View();
        }

        public ActionResult ProcessingOrder(int? page)
        {
            ViewData["OrderStatusTextMapping"] = GetOrderStatusTextMapping();

            int pageNum = page.HasValue ? page.Value : 1;
            PagedList<BlackMamba.Billing.Models.Payments.Order> orders = this._paymentUISvc.GetPaymentingOrder(pageNum-1);
            orders.PageIndex = pageNum;

            ViewBag.Title = "请求中订单";
            ViewData["Statu"] = (int)OrderStatus.Processing;
            return View("OrderManage", orders);
        }

        public ActionResult GetOrderLine(string orderNo, int page=1)
        {
            List<OrderLine> orderLine = _paymentUISvc.GetOrderLine(orderNo);
            ViewBag.Title = TempData["ParentTitle"];
            
            return View(orderLine);
        }

        #region Helper

        protected Dictionary<string, string> GetOrderStatusTextMapping()
        {

            var statuNames = Enum.GetNames(typeof(BlackMamba.Billing.Models.Payments.OrderStatus));
            Dictionary<string, string> OrderStatusTextMapping = new Dictionary<string, string>();
            foreach (var name in statuNames)
            {
                var text = "订单已初始化";

                if (name == OrderStatus.Created.ToString())
                {
                    text = "订单已创建,等待用户处理";
                }
                else if (name == OrderStatus.Successed.ToString())
                {
                    text = "从第三方充值流程全部成功";
                }
                else
                    if (name == OrderStatus.Failed.ToString())
                    { text = "充值失败"; }
                    else if (name == OrderStatus.Canceled.ToString())
                    { text = "订单已取消"; }
                    else if (name == OrderStatus.Expired.ToString())
                    { text = "订单已过期"; }
                    else if (name == OrderStatus.Processing.ToString())
                    { text = "平台已接受充值,未加值到游戏"; }
                    else if (name == OrderStatus.Stoped.ToString())
                    { text = "平台已接受充值,加值到游戏未成功.充值金额留存于平台"; }
                    else if (name == OrderStatus.Exceed.ToString())
                    { text = "平台充值超过订单"; }
                    else if (name == OrderStatus.PartlySuccess.ToString())
                    { text = "部分充值"; }
                    else if (name == OrderStatus.Refunded.ToString())
                    { text = "从第三方充值流程全部成功"; }
                    else if (name == OrderStatus.Complete.ToString())
                    { text = "充值流程完全成功，并且已通知游戏服务器"; }


                OrderStatusTextMapping[name] = text;
            }

            return OrderStatusTextMapping;
        }

        #endregion
    }
}
