using System;
using System.Web.Mvc;
using BlackMamba.Framework.Core;
using NewOrder = BlackMamba.Billing.Models.Payments.Order;
using System.Text;
using BlackMamba.Billing.Domain;
using BlackMamba.Billing.Domain.Services;
using BlackMamba.Billing.Models.Payments;
using BlackMamba.Billing.Domain.Common;
using BlackMamba.Billing.Domain.ViewModels.Billing;
using BlackMamba.Framework.Core.Security;
using System.Collections.Generic;

namespace BlackMamba.Billing.Website.Controllers
{
    public class YeepayController : MvcControllerBase
    {
        public IMailService PaymentsService { get; set; }
        //public IOrderService OrderService { get; set; }
        public YeepayController(IMailService paymentsService)//, IOrderService orderService)
        {
            PaymentsService = paymentsService;
            // OrderService = orderService;
        }

        //public ActionResult YeepayPayment(string orderNo, string imsi)
        //{
        //    Response.ContentEncoding = System.Text.Encoding.GetEncoding("gb2312");

        //    var callBackUrl = ConfigKeys.YEEPAY_CALLBACK_URL.ConfigValue();
        //    var customerOrder = OrderService.GetCustomerOrder(orderNo);
        //    if (customerOrder == null)
        //    {
        //        return Content("订单号错误！");
        //    }
        //    if (customerOrder.Status == OrderStatus.Successed)
        //    {
        //        return Content("已完成支付订单！");
        //    }
        //    ViewData["BuyUrl"] = com.yeepay.icc.Buy.GetBuyUrl();
        //    ViewData["p1_MerId"] = com.yeepay.icc.Buy.GetMerId();
        //    ViewData["CallBackUrl"] = callBackUrl;
        //    var amount = customerOrder.Payed != null ? customerOrder.Amount - customerOrder.Payed : customerOrder.Amount;
        //    ViewData["Amount"] = amount.ToString();
        //    ViewData["hmac"] = com.yeepay.icc.Buy.CreateBuyHmac(customerOrder.OrderNo, amount.ToString(), customerOrder.Currency, customerOrder.ProductName, customerOrder.ProductType, customerOrder.ProductDescription, callBackUrl, "0", customerOrder.MerchantExtentionalDescription, "", "1");

        //    return View(customerOrder);
        //}

        public ActionResult YeepayPayments(float? Amount, string productName = "", string productType = "", string productDescription = "", string MerchantExtentionalDescription = "", string userId = "", string userName = "")
        {
            var urlSig = new UrlSignature(this.RequestRepository, Encoding.UTF8);
            if (!urlSig.IsValid())
            {
                return Content("签名错误");
            }
            var callBackUrl = "callBackUrl".UrlDecodeFromRawQuery(this.RequestRepository);
            BlackMamba.Billing.Models.Payments.Order order = new BlackMamba.Billing.Models.Payments.Order();

            ViewData["BuyUrl"] = com.yeepay.icc.Buy.GetBuyUrl();
            ViewData["p1_MerId"] = com.yeepay.icc.Buy.GetMerId();

            if (!callBackUrl.IsNullOrEmpty() && Amount != 0.00f && !userId.IsNullOrEmpty() && !userName.IsNullOrEmpty())
            {
                order = PaymentsService.RequestBankCardPayment(Amount.GetValueOrDefault(), productName, productType, productDescription, MerchantExtentionalDescription, callBackUrl, userId, userName);
                if (order.OrderNo.IsNullOrEmpty())
                {
                    return Content("服务器错误");
                }
                ViewData["CallBackUrl"] = callBackUrl;
                ViewData["Amount"] = order.Amount.ToString();
                ViewData["hmac"] = com.yeepay.icc.Buy.CreateBuyHmac(order.OrderNo, order.Amount.ToString(), order.Currency, order.ProductName, order.ProductType, order.ProductDescription, callBackUrl, "0", order.MerchantExtentionalDescription, "", "1");
            }
            else
            {
                return Content("参数错误");
            }

            return View(order);
        }

        public ActionResult BankCallBack()
        {
            var p1_MerId = "p1_MerId".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var r0_Cmd = "r0_Cmd".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var r1_Code = "r1_Code".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var r2_TrxId = "r2_TrxId".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var r3_Amt = "r3_Amt".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var r4_Cur = "r4_Cur".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var r5_Pid = "r5_Pid".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var r6_Order = "r6_Order".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var r7_Uid = "r7_Uid".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var r8_MP = "r8_MP".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var r9_BType = "r9_BType".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var rb_BankId = "rb_BankId".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var ro_BankOrderId = "ro_BankOrderId".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var rp_PayDate = "rp_PayDate".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var rq_CardNo = "rq_CardNo".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var ru_Trxtime = "ru_Trxtime".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var hmac = "hmac".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);

            var result = this.PaymentsService.YeepayBankCardCallBack(p1_MerId, r0_Cmd, r1_Code, r2_TrxId, r3_Amt, r4_Cur, r5_Pid, r6_Order, r7_Uid, r8_MP, r9_BType, rb_BankId, ro_BankOrderId, rp_PayDate, rq_CardNo, ru_Trxtime, hmac);
            return Content(result);
        }

        public ActionResult YeepayCardPayments(string orderNo, float cardAmount, string cardPwd, string cardType)
        {
            var result = string.Empty;
            var i = 0;
            do
            {
                result = null;// PaymentsService.YeepayCardPayments(orderNo, cardAmount, cardPwd, cardType);
                i++;
            } while (string.IsNullOrEmpty(result) || i > 5);

            if (string.IsNullOrEmpty(result))
            {
                return Content("支付出现异常");
            }

            return Content(result);

        }

        public ActionResult YeepayCallBack()
        {
            var p1_merid = "p1_MerId".UrlDecodeFromRawQuery(RequestRepository);
            var r0_cmd = "r0_Cmd".UrlDecodeFromRawQuery(RequestRepository);
            var r1_code = "r1_Code".UrlDecodeFromRawQuery(RequestRepository);
            var r2_trxid = "r2_TrxId".UrlDecodeFromRawQuery(RequestRepository);
            var r3_amt = "r3_Amt".UrlDecodeFromRawQuery(RequestRepository);
            var r4_cur = "r4_Cur".UrlDecodeFromRawQuery(RequestRepository);
            var r5_pid = "r5_Pid".UrlDecodeFromRawQuery(RequestRepository);
            var r6_order = "r6_Order".UrlDecodeFromRawQuery(RequestRepository);
            var r7_uid = "r7_Uid".UrlDecodeFromRawQuery(RequestRepository);
            var r8_mp = "r8_MP".UrlDecodeFromRawQuery(RequestRepository);
            var r9_btype = "r9_BType".UrlDecodeFromRawQuery(RequestRepository);
            var rp_paydate = "rp_PayDate".UrlDecodeFromRawQuery(RequestRepository);
            var hmac = "hmac".UrlDecodeFromRawQuery(RequestRepository);

            var paymentDetails = PaymentsService.YeepayCallBack(p1_merid, r0_cmd, r1_code, r2_trxid, r3_amt, r4_cur, r5_pid, r6_order, r7_uid, r8_mp, r9_btype, rp_paydate, hmac);
            if (paymentDetails == null)
            {
                return Content("");
            }
            if (paymentDetails.ResponseType.Equals("0"))
            {
                return Content("交易签名无效");
            }
            if (paymentDetails.ResponseType.Equals("1"))
            {
                return Content("支付成功! 订单号：" + paymentDetails.OrderNo + ",支付金额：" + paymentDetails.Amount + ",交易流水号：" + paymentDetails.PaymentNo);
            }
            if (paymentDetails.ResponseType.Equals("2"))
            {
                return Content("SUCCESS");
            }
            if (paymentDetails.ResponseType.Equals("3"))
            {
                return Content("支付失败：" + paymentDetails.ErrMsg);
            }
            return Content("");
        }

        public ActionResult YeepayCardCallBack()
        {
            var r0_Cmd = "r0_Cmd".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var r1_Code = "r1_Code".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var p1_MerId = "p1_MerId".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var p2_Order = "p2_Order".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var p3_Amt = "p3_Amt".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var p4_FrpId = "p4_FrpId".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var p5_CardNo = "p5_CardNo".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var p6_confirmAmount = "p6_confirmAmount".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var p7_realAmount = "p7_realAmount".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var p8_cardStatus = "p8_cardStatus".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var p9_MP = "p9_MP".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var pb_BalanceAmt = "pb_BalanceAmt".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var pc_BalanceAct = "pc_BalanceAct".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
            var hmac = "hmac".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);

            var result = PaymentsService.YeepayCardCallBack(r0_Cmd, r1_Code, p1_MerId, p2_Order, p3_Amt, p4_FrpId, p5_CardNo, p6_confirmAmount, p7_realAmount, p8_cardStatus, p9_MP, pb_BalanceAmt, pc_BalanceAct, hmac);
            return Content(result);
        }

        //public ActionResult ReissueOrder(string orderNo, string imsi)
        //{
        //    Func<bool> checkParameter = () => this.CheckRequiredParams(imsi)() && !string.IsNullOrEmpty(orderNo);

        //    Func<StringView> ReissueResult = () => new StringView(PaymentsService.ReissueOrder(orderNo));

        //    var result = BuildResult(checkParameter, ReissueResult);
        //    return Content(result.ToString());
        //}

        //public ActionResult RefundMoney(string yeepayPayNo, string refundDesc, string imsi)
        //{
        //    Func<bool> checkParameter = () => this.CheckRequiredParams(imsi)() && !string.IsNullOrEmpty(yeepayPayNo);

        //    Func<StringView> RefundResult = () => new StringView(PaymentsService.RefundMoney(yeepayPayNo, refundDesc));

        //    var actionResult = BuildResult(checkParameter, RefundResult);
        //    return Content(actionResult.ToString());
        //}

        public ActionResult CardPay(int? cardType, string cardNo, string cardPassword, float? cardAmount, float? Amount, string productName = "", string productType = "", string productDescription = "", string MerchantExtentionalDescription = "", string userId = "", string userName = "")
        {
            var callBackUrl = "callBackUrl".UrlDecodeFromRawQuery(this.RequestRepository);

            Func<bool> checkParameter = () => cardType.GetValueOrDefault() != 0 && !string.IsNullOrEmpty(cardNo)
                                            && !string.IsNullOrEmpty(cardPassword) && cardAmount.GetValueOrDefault() != 0.0f
                                            && !string.IsNullOrEmpty(callBackUrl);

            Func<OrderViewModel> cardPay = () => PaymentsService.RequestCardPayment(cardType.GetValueOrDefault(), cardNo,
                cardPassword, cardAmount.GetValueOrDefault(), Amount.GetValueOrDefault(), productName,
                productType, productDescription,
                MerchantExtentionalDescription,
                callBackUrl, userId, userName);

            var actionResult = BuildResult(checkParameter, cardPay, false);
            return Content(actionResult.ToString());
        }

        public ActionResult NewCardPay(int? cardType, string cardNo, string cardPassword, float? cardAmount, float? Amount, string productName = "", string productType = "", string productDescription = "", string MerchantExtentionalDescription = "", string userId = "", string userName = "")
        {
            var callBackUrl = "";//"callBackUrl".UrlDecodeFromRawQuery(this.RequestRepository);

            Func<bool> checkParameter = () => cardType.GetValueOrDefault() != 0 && !string.IsNullOrEmpty(cardNo)
                                            && !string.IsNullOrEmpty(cardPassword) && cardAmount.GetValueOrDefault() != 0.0f;

            Func<OrderViewModel> cardPay = () => PaymentsService.RequestCardPayment(cardType.GetValueOrDefault(), cardNo,
                cardPassword, cardAmount.GetValueOrDefault(), Amount.GetValueOrDefault(), productName,
                productType, productDescription,
                MerchantExtentionalDescription,
                callBackUrl, userId, userName);

            var actionResult = BuildResult(checkParameter, cardPay, false);
            return Content(actionResult.ToString());
        }

        public ActionResult CheckPaymentStatus(string orderNo)
        {
            var orderStatus = PaymentsService.CheckPaymentStatus(orderNo);
            return Content(orderStatus);                              //Successed = 2
        }

        //public ActionResult YeepaySDKCallBack()
        //{
        //    var p1_MerId = "p1_MerId".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
        //    var r0_Cmd = "r0_Cmd".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
        //    var r1_Code = "r1_Code".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
        //    var r2_TrxId = "r2_TrxId".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
        //    var r3_Amt = "r3_Amt".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
        //    var r4_Cur = "r4_Cur".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
        //    var r5_Pid = "r5_Pid".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
        //    var r6_Order = "r6_Order".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
        //    var r7_Uid = "r7_Uid".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
        //    var r8_MP = "r8_MP".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
        //    var r9_BType = "r9_BType".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
        //    var rb_BankId = "rb_BankId".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
        //    var ro_BankOrderId = "ro_BankOrderId".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
        //    var rp_PayDate = "rp_PayDate".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
        //    var rq_CardNo = "rq_CardNo".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
        //    var ru_Trxtime = "ru_Trxtime".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);
        //    var hmac = "hmac".UrlDecodeFromRawQuery(RequestRepository, EncodingNames.GBK);

        //    LogManager.GetLogger("YeepaySDKCallBackTest").Info(string.Format("p1_MerId={0}&r0_Cmd={1}&r1_Code={2}&r2_TrxId={3}&r3_Amt={4}&r4_Cur={5}&r5_Pid={6}&r6_Order={7}&r7_Uid={8}&r8_MP={9}&r9_BType={10}&rb_BankId={11}&ro_BankOrderId={12}&rp_PayDate={13}&rq_CardNo={14}&ru_Trxtime={15}&hmac={16}",
        //            p1_MerId, r0_Cmd, r1_Code, r2_TrxId, r3_Amt, r4_Cur, r5_Pid, r6_Order, r7_Uid, r8_MP, r9_BType, rb_BankId, ro_BankOrderId, rp_PayDate, rq_CardNo, ru_Trxtime, hmac));

        //    var result = this.PaymentsService.YeepaySDKCallBack(p1_MerId, r0_Cmd, r1_Code, r2_TrxId, r3_Amt, r4_Cur, r5_Pid, r6_Order, r7_Uid, r8_MP, r9_BType, rb_BankId, ro_BankOrderId, rp_PayDate, rq_CardNo, ru_Trxtime, hmac);
        //    return Content(result);
        //}
    }
}
