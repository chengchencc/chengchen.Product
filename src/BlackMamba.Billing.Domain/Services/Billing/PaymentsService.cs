using System;
using System.Text;
using com.yeepay.cmbn;
using com.yeepay.icc;
using BlackMamba.Framework.Core;
using BlackMamba.Framework.RedisMapper;
using SubSonic.Oracle.Repository;
using Order = BlackMamba.Billing.Models.Payments.Order;
using System.Xml;
//using SubSonicOracle = SubSonic.Oracle.Repository;
using AlipayFunction = Alipay.Class.Function;
using Alipay.Class;
using System.Collections.Generic;
using BlackMamba.Billing.Models.Payments;
using BlackMamba.Billing.Domain.ViewModels.Billing;
using BlackMamba.Billing.Models;
using BlackMamba.Billing.Models.Billing;
using BlackMamba.Billing.Domain.Common;
using NLog;
using BlackMamba.Framework.SubSonic.Oracle;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace BlackMamba.Billing.Domain.Services
{
    public class PaymentsService : BlackMamba.Billing.Domain.Services.IMailService
    {
        const string SEQUENCE_ORDER_COUNT = "SEQ:ORDERCOUNT";
        public IRepository Repository { get; set; }
        public IRedisService RedisService { get; set; }
        private object sync = new object();
        public IYeepayService YeepayService;
        public IRESTfulClient RESTfulCient;

        public PaymentsService(IRedisService redisService, IYeepayService yeepayService, IRESTfulClient restfulCient)
        {
            this.Repository = new SimpleRepository(ConnectionStrings.KEY_ORACLE_GENERAL, SimpleRepositoryOptions.RunMigrations);
            this.RedisService = redisService;
            this.YeepayService = yeepayService;
            this.RESTfulCient = restfulCient;
        }

        #region Yeepay Card Payment
        /// <summary>
        /// 部分充值 其余留原卡号内
        /// </summary>
        /// <param name="cardPayment"></param>
        /// <returns></returns>
        public SZXResult YeepayCardPartialPayments(CardPayment cardPayment)
        {
            SZXResult result = null;
            if (cardPayment != null)
            {
                var p2_Order = cardPayment.OrderNo;
                var p3_Amt = cardPayment.Amount;
                var p4_verifyAmt = "true"; // false : 不校验金额 有多少冲多少
                var p5_Pid = cardPayment.ProductName;
                var p6_Pcat = cardPayment.ProductType;
                var p7_Pdesc = cardPayment.ProductDescription;
                var p8_Url = ConfigKeys.YEEPAY_CARD_CALLBACK_URL.ConfigValue();
                var pa_MP = cardPayment.MerchantExtentionalDescription;
                var pa7_cardAmt = cardPayment.CardAmount;
                var pa8_cardNo = cardPayment.CardNo;
                var pa9_cardPwd = cardPayment.CardPassword;
                var pd_FrpId = cardPayment.CardType.ToString();
                var pr_NeedResponse = "1";
                var pz_userId = "";
                var pz1_userRegTime = "";

                try
                {
                    //非银行卡专业版正式使用
                    result = YeepayService.AnnulCard(p2_Order, p3_Amt.ToString(), p4_verifyAmt, p5_Pid, p6_Pcat, p7_Pdesc, p8_Url,
                    pa_MP, pa7_cardAmt.ToString(), pa8_cardNo, pa9_cardPwd, pd_FrpId, pr_NeedResponse, pz_userId.ToString(), pz1_userRegTime);
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger("ErrorLogger").Error(string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
                }
            }

            return result;
        }

        /// <summary>
        /// varifyAmount为false 卡中有多少冲多少
        /// </summary>
        /// <param name="cardPayment"></param>
        /// <returns></returns>
        public SZXResult YeepayCardPayments(CardPayment cardPayment)
        {
            SZXResult result = null;
            if (cardPayment != null)
            {

                if (cardPayment.Amount == 0.0f)
                {
                    cardPayment.Amount = cardPayment.CardAmount; // 按面额全部充掉
                }

                var p2_Order = cardPayment.OrderNo;
                var p3_Amt = cardPayment.Amount;
                var p4_verifyAmt = "false"; // false : 不校验金额 有多少冲多少
                var p5_Pid = cardPayment.ProductName;
                var p6_Pcat = cardPayment.ProductType;
                var p7_Pdesc = cardPayment.ProductDescription;
                var p8_Url = ConfigKeys.YEEPAY_CARD_CALLBACK_URL.ConfigValue();
                var pa_MP = cardPayment.MerchantExtentionalDescription;
                var pa7_cardAmt = cardPayment.CardAmount;
                var pa8_cardNo = cardPayment.CardNo;
                var pa9_cardPwd = cardPayment.CardPassword;
                var pd_FrpId = cardPayment.CardType.ToString();
                var pr_NeedResponse = "1";
                var pz_userId = "";
                var pz1_userRegTime = "";

                try
                {
                    //非银行卡专业版正式使用
                    result = YeepayService.AnnulCard(p2_Order, p3_Amt.ToString(), p4_verifyAmt, p5_Pid, p6_Pcat, p7_Pdesc, p8_Url,
                    pa_MP, pa7_cardAmt.ToString(), pa8_cardNo, pa9_cardPwd, pd_FrpId, pr_NeedResponse, pz_userId.ToString(), pz1_userRegTime);
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger("ErrorLogger").Error(string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
                }
            }

            return result;
        }

        public string YeepayCardCallBack(string r0_Cmd, string r1_Code, string p1_MerId, string p2_Order, string p3_Amt, string p4_FrpId, string p5_CardNo, string p6_confirmAmount, string p7_realAmount, string p8_cardStatus, string p9_MP, string pb_BalanceAmt, string pc_BalanceAct, string hmac)
        {
            //SZXCallbackResult result = SZX.VerifyCallback(r0_Cmd,r1_Code,p1_MerId,p2_Order, p3_Amt, p4_FrpId, p5_CardNo,p6_confirmAmount, p7_realAmount, p8_cardStatus,p9_MP,pb_BalanceAmt,pc_BalanceAct,hmac);
            SZXCallbackResult result = YeepayService.VerifyCallback(r0_Cmd, r1_Code, p1_MerId, p2_Order, p3_Amt, p4_FrpId, p5_CardNo, p6_confirmAmount, p7_realAmount, p8_cardStatus, p9_MP, pb_BalanceAmt, pc_BalanceAct, hmac);
            StringBuilder sb = new StringBuilder();


            if (string.IsNullOrEmpty(result.ErrMsg))
            {
                // 使用应答机制时 必须回写success
                sb.Append("SUCCESS");
                //在接收到支付结果通知后，判断是否进行过业务逻辑处理，不要重复进行业务逻辑处理
                if (result.R1_Code == "1")
                {
                    //SuccessfullyCardCallBackLogic(result);
                    sb.Append("<BR>非银行卡支付成功");
                    sb.Append("<BR>商户订单号:" + result.P2_Order);
                    sb.Append("<BR>实际扣款金额(商户收到该返回数据后,一定用自己数据库中存储的金额与该金额进行比较):" + result.P3_Amt);
                }
                else
                {
                    sb.Append("交易失败!");
                }

                this.CardCallBackLogic(result);
            }
            else
            {
                sb.Append("交易签名无效!");
                sb.Append("<BR>YeePay-HMAC:" + result.Hmac);
                sb.Append("<BR>LocalHost:" + result.ErrMsg);
                LogManager.GetLogger("ErrorLogger").Error(string.Format("交易签名无效: {0}", p2_Order));
            }

            return sb.ToString();
        }

        public OrderViewModel RequestCardPayment(int cardType, string cardNo, string cardPassword, float cardAmount, float Amount, string productName, string productType, string productDescription, string MerchantExtentionalDescription, string callBackUrl, string userId, string userName)
        {
            OrderViewModel res = new OrderViewModel();

            if (Amount == 0 && cardAmount > 0) Amount = cardAmount;

            Order order = CreateOrder(Amount, MerchantExtentionalDescription, productDescription, productName, productType, callBackUrl, userId, userName, (int)PaymentType.Yeepay, cardType, cardNo, cardPassword, cardAmount);
            CardPayment cardPayment = default(CardPayment);
            if (order != null && order.OrderNo != null)
            {
                long orderId = SaveOrder(order);
                order.Id = (Int32)orderId;

                cardPayment = CreateCardPayment(order, cardType, cardNo, cardPassword, cardAmount, Amount, productName, productType, productDescription, MerchantExtentionalDescription);
                if (cardPayment != null && cardPayment.OrderNo != null)
                {
                    SaveCardPayment(cardPayment);
                }
            }
            if (order != null && cardPayment != null && order.OrderNo != null && cardPayment.OrderNo != null)
            {
                res.OrderNo = order.OrderNo;
            }

            return res;
        }

        public string CheckPaymentStatus(string orderNo)
        {
            var order = Repository.Single<Order>(s=>s.OrderNo == orderNo&&s.Status == EntityStatus.Enabled);
            if (order != null && order.CheckCount < "CheckCount".ConfigValue().ToInt32())
            {
                order.CheckCount++;
                if (order.OrderStatus == OrderStatus.Successed || order.OrderStatus == OrderStatus.Stoped || order.OrderStatus == OrderStatus.Complete )
                {   
                    order.Status = EntityStatus.Disabled;
                }
                Repository.Update<Order>(order);
                return ((int)order.OrderStatus).ToString()+"|"+order.Amount;
            }
            else
            {
                order.Status = EntityStatus.Disabled;
                Repository.Update<Order>(order);
                return ((int)OrderStatus.Invalid).ToString() + "|" + 0;
            }
        }

        #endregion

        #region Yeepay Web Payment
        public PaymentDetails YeepayCallBack(string p1_merid, string r0_cmd, string r1_code, string r2_trxid, string r3_amt, string r4_cur, string r5_pid, string r6_order, string r7_uid, string r8_mp, string r9_btype, string rp_paydate, string hmac)
        {
            BuyCallbackResult result = Buy.VerifyCallback(p1_merid, r0_cmd, r1_code, r2_trxid, r3_amt, r4_cur, r5_pid, r6_order, r7_uid, r8_mp, r9_btype, rp_paydate, hmac);
            if (string.IsNullOrEmpty(result.ErrMsg))
            {
                //在接收到支付结果通知后，判断是否进行过业务逻辑处理，不要重复进行业务逻辑处理
                if (result.R1_Code == "1")
                {
                    if (result.R9_BType == "1")
                    {
                        //  callback方式:浏览器重定向
                        SuccessfullyCallBackLogic(result);

                        // todo: 这儿重复的代码太多
                        // 需要加一些测试与变量
                        var paymentDetails = new PaymentDetails();
                        paymentDetails.ErrMsg = result.ErrMsg;
                        paymentDetails.OrderNo = result.R6_Order;
                        paymentDetails.PaymentDate = result.Rp_PayDate.ToExactDateTime(DateTimeFormat.yyyyMMddHHmmss);
                        paymentDetails.PaymentNo = result.R2_TrxId;
                        paymentDetails.Amount = result.R3_Amt;
                        paymentDetails.PaymentStatus = PaymentStatus.SUCCESS.ToString();
                        paymentDetails.ResponseType = "1";
                        return paymentDetails;
                    }
                    else if (result.R9_BType == "2")
                    {
                        // * 如果是服务器返回则需要回应一个特定字符串'SUCCESS',且在'SUCCESS'之前不可以有任何其他字符输出,保证首先输出的是'SUCCESS'字符串
                        SuccessfullyCallBackLogic(result);

                        var paymentDetails = new PaymentDetails();
                        paymentDetails.ErrMsg = result.ErrMsg;
                        paymentDetails.OrderNo = result.R6_Order;
                        paymentDetails.PaymentDate = result.Rp_PayDate.ToExactDateTime(DateTimeFormat.yyyyMMddHHmmss);
                        paymentDetails.PaymentNo = result.R2_TrxId;
                        paymentDetails.Amount = result.R3_Amt;
                        paymentDetails.PaymentStatus = PaymentStatus.SUCCESS.ToString();
                        paymentDetails.ResponseType = "2";
                        return paymentDetails;
                    }
                    return null;
                }
                else
                {
                    var paymentDetails = new PaymentDetails();
                    paymentDetails.ErrMsg = result.ErrMsg;
                    paymentDetails.OrderNo = result.R6_Order;
                    paymentDetails.PaymentDate = result.Rp_PayDate.ToExactDateTime(DateTimeFormat.yyyyMMddHHmmss);
                    paymentDetails.PaymentNo = result.R2_TrxId;
                    paymentDetails.Amount = result.R3_Amt;
                    paymentDetails.PaymentStatus = PaymentStatus.FAILED.ToString();
                    paymentDetails.ResponseType = "3";
                    return paymentDetails;
                }
            }
            else
            {
                var paymentDetails = new PaymentDetails();
                paymentDetails.ResponseType = "0";
                return paymentDetails;
            }
        }

        //public string YeepaySDKCallBack(string p1_MerId, string r0_Cmd, string r1_Code, string r2_TrxId, string r3_Amt, string r4_Cur, string r5_Pid, string r6_Order, string r7_Uid, string r8_MP, string r9_BType, string rb_BankId, string ro_BankOrderId, string rp_PayDate, string rq_CardNo, string ru_Trxtime, string hmac)
        //{
        //    SDKCallbackResult result = YeepayService.VerifySDKCallBack(p1_MerId, r0_Cmd, r1_Code, r2_TrxId, r3_Amt, r4_Cur, r5_Pid, r6_Order, r7_Uid, r8_MP, r9_BType, rb_BankId, ro_BankOrderId, rp_PayDate, rq_CardNo, ru_Trxtime, hmac);
        //    StringBuilder sb = new StringBuilder();

        //    if (result.ErrMsg.IsNullOrEmpty())
        //    {
        //        sb.Append("SUCCESS");
        //        if (result.R1_Code == "1")
        //        {
        //            sb.Append("<BR>支付成功");
        //            sb.Append("<BR>商户订单号:" + result.R6_Order);
        //            sb.Append("<BR>实际扣款金额(商户收到该返回数据后,一定用自己数据库中存储的金额与该金额进行比较):" + result.R3_Amt);
        //        }
        //        else
        //        {
        //            sb.Append("交易失败!");
        //        }
        //        this.CallBackLogic_SDK(result);
        //    }
        //    else
        //    {
        //        sb.Append("交易签名无效!");
        //        sb.Append("<BR>YeePay-HMAC:" + result.Hmac);
        //        sb.Append("<BR>LocalHost:" + result.ErrMsg);
        //    }
        //    return sb.ToString();
        //}

        public void YeepayCallBackTransaction(OrderLine orderLine)
        {
            lock (this.sync)
            {
                var record = Repository.Find<OrderLine>(s => s.YeepayPayNo == orderLine.YeepayPayNo && s.PaymentStatus == "SUCCESS");
                if (record == null || record.Count == 0)
                {
                    var customerOrder = Repository.Single<CustomerOrder>(s => s.OrderNo == orderLine.OrderNo);
                    if (customerOrder != null)
                    {
                        //using (TransactionScope scope = new TransactionScope())
                        //{
                        //    using (SharedDbConnectionScope sdcs = new SharedDbConnectionScope())
                        //    {
                        //this.Repository = new SimpleRepository(ConnectionStrings.KEY_ORACLE_GENERAL, SimpleRepositoryOptions.RunMigrations);

                        Repository.Add<OrderLine>(orderLine);
                        ChangeBalance(customerOrder.CustomerId, orderLine.PaymentAmount);
                        UpdateCustomerOrder(orderLine.OrderNo, orderLine.PaymentAmount, customerOrder.CustomerId);
                        //scope.Complete();
                        //    }
                        //}
                    }
                }
            }
        }

        /// <summary>
        /// change Balance for Account by customerId
        /// </summary>
        /// <param name="customerId">By this to get details for any customer's balance</param>
        /// <param name="Increment">Increment 正值为加，即充值； 负值为减，即花费。</param>
        /// <returns>FinancialAccount</returns>
        public FinancialAccount ChangeBalance(int customerId, float Increment)//
        {
            FinancialAccount financialAccount = null;
            try
            {
                financialAccount = Repository.Single<FinancialAccount>(s => s.CustomerId == customerId);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ErrorLogger").Error(string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
            }

            if (financialAccount != null)
            {
                financialAccount.Balance = financialAccount.Balance + Increment;
                Repository.Update<FinancialAccount>(financialAccount);
                return financialAccount;
            }
            else
            {
                financialAccount = new FinancialAccount()
                {
                    Balance = Increment,
                    CustomerId = customerId,
                    CreatedDate = DateTime.Now
                };
                Repository.Add<FinancialAccount>(financialAccount);
                return financialAccount;
            }
        }

        public CustomerOrder UpdateCustomerOrder(string orderNo, float payedBalance, int customerId)
        {
            var order = Repository.Single<CustomerOrder>(s => s.OrderNo == orderNo);
            if (order != null)
            {
                if (order.Payed == null)
                {
                    order.Payed = 0;
                }
                var surplusAmount = order.Amount - order.Payed;
                if (surplusAmount == payedBalance)
                {
                    order.Payed = order.Amount;
                    order.Status = OrderStatus.Successed;
                }
                if (surplusAmount > payedBalance)
                {
                    order.Payed = order.Payed + payedBalance;
                    order.Status = OrderStatus.PartlySuccess;
                }
                if (surplusAmount < payedBalance)
                {
                    order.Payed = payedBalance + order.Payed;
                    order.Status = OrderStatus.Exceed;
                }
                Repository.Update<CustomerOrder>(order);
            }
            else
            {//假如查找不到订单号，新加一个已经完成的订单
                order = new CustomerOrder();
                order.OrderNo = orderNo;
                order.Amount = payedBalance;
                order.Status = OrderStatus.Successed;
                order.CreateDate = DateTime.Now;
                order.CustomerId = customerId;
                order.Payed = payedBalance;
                Repository.Add<CustomerOrder>(order);
            }
            return order;
        }

        public string ReissueOrder(string orderNo)
        {
            ReissueOrderStatus result = ReissueOrderStatus.Untreated;

            BuyQueryOrdDetailResult QueryResult = Buy.QueryOrdDetail(orderNo);
            if (QueryResult.R1_Code == "1")
            {//查询成功
                if (QueryResult.Rb_PayStatus == PaymentStatus.SUCCESS.ToString())
                {
                    OrderLine orderLine = new OrderLine()
                    {
                        OrderNo = QueryResult.R6_Order,
                        PaymentAmount = float.Parse(QueryResult.R3_Amt),
                        Currency = QueryResult.R4_Cur,
                        PaymentStatus = PaymentStatus.SUCCESS.ToString(),
                        YeepayPayNo = QueryResult.R2_TrdId,
                    };
                    YeepayCallBackTransaction(orderLine);
                    result = ReissueOrderStatus.Successed;// "补单成功";
                }
                if (QueryResult.Rb_PayStatus == PaymentStatus.INIT.ToString())
                {
                    result = ReissueOrderStatus.NonPament;// "订单未支付";
                }
                if (QueryResult.Rb_PayStatus == PaymentStatus.CANCELED.ToString())
                {
                    CustomerOrder customerOrder = new CustomerOrder();
                    customerOrder = Repository.Single<CustomerOrder>(orderNo);
                    if (customerOrder != null)
                    {
                        customerOrder.Status = OrderStatus.Canceled;
                        Repository.Update<CustomerOrder>(customerOrder);
                    }
                    result = ReissueOrderStatus.CancledOrder;// "已取消的订单";
                }
            }
            else
            {//查询失败
                result = ReissueOrderStatus.UnknownOrder; //"无此订单信息";
            }
            return result.ToString();
        }

        public string RefundMoney(string yeepayPayNo, string RefundDesc)
        {
            var orderLine = Repository.Single<OrderLine>(s => s.YeepayPayNo == yeepayPayNo);
            if (orderLine != null)
            {
                if (orderLine.Currency == "RMB")
                {
                    orderLine.Currency = "CNY";
                }
                BuyRefundOrdResult RefundResult = Buy.RefundOrd(orderLine.YeepayPayNo, orderLine.PaymentAmount.ToString(), orderLine.Currency, RefundDesc);
                if (RefundResult.R1_Code == "1")
                {
                    //update CustomerOrder
                    var customerOrder = Repository.Single<CustomerOrder>(s => s.OrderNo == orderLine.OrderNo);
                    customerOrder.Payed = customerOrder.Payed - orderLine.PaymentAmount;
                    customerOrder.Status = OrderStatus.Refunded;
                    Repository.Update<CustomerOrder>(customerOrder);

                    //update FinancialAccount
                    var financialAccount = Repository.Single<FinancialAccount>(s => s.CustomerId == customerOrder.CustomerId);
                    financialAccount.Balance = financialAccount.Balance - orderLine.PaymentAmount;
                    Repository.Update<FinancialAccount>(financialAccount);

                    //update order line
                    orderLine.PaymentStatus = PaymentStatus.REFUNDED.ToString();
                    orderLine.Description = RefundDesc;
                    Repository.Update<OrderLine>(orderLine);
                    return "true";
                }
            }
            return "false";
        }

        public Order RequestBankCardPayment(float Amount, string productName, string productType, string productDescription, string MerchantExtentionalDescription, string callBackUrl, string userId, string userName)
        {
            long orderId;

            Order order = CreateOrder(Amount, MerchantExtentionalDescription, productDescription, productName, productType, callBackUrl, userId, userName, (int)PaymentType.Yeepay);
            if (order != null && order.OrderNo != null)
            {
                orderId = SaveOrder(order);
                order.Id = (int)orderId;
            }

            return order;
        }

        public string YeepayBankCardCallBack(string p1_MerId, string r0_Cmd, string r1_Code, string r2_TrxId, string r3_Amt, string r4_Cur, string r5_Pid, string r6_Order, string r7_Uid, string r8_MP, string r9_BType, string rb_BankId, string ro_BankOrderId, string rp_PayDate, string rq_CardNo, string ru_Trxtime, string hmac)
        {
            SDKCallbackResult result = YeepayService.VerifySDKCallBack(p1_MerId, r0_Cmd, r1_Code, r2_TrxId, r3_Amt, r4_Cur, r5_Pid, r6_Order, r7_Uid, r8_MP, r9_BType, rb_BankId, ro_BankOrderId, rp_PayDate, rq_CardNo, ru_Trxtime, hmac);
            StringBuilder sb = new StringBuilder();

            if (result.ErrMsg.IsNullOrEmpty())
            {
                sb.Append("SUCCESS");
                if (result.R1_Code == "1")
                {
                    sb.Append("<BR>支付成功");
                    sb.Append("<BR>商户订单号:" + result.R6_Order);
                    sb.Append("<BR>实际扣款金额(商户收到该返回数据后,一定用自己数据库中存储的金额与该金额进行比较):" + result.R3_Amt);
                }
                else
                {
                    sb.Append("交易失败!");
                }
                this.BankCardCallBack(result);
            }
            else
            {
                sb.Append("交易签名无效!");
                sb.Append("<BR>YeePay-HMAC:" + result.Hmac);
                sb.Append("<BR>LocalHost:" + result.ErrMsg);
            }
            return sb.ToString();
        }
        #endregion

        #region Alipay

        public string CreateOrder(string productName, float amount, string productDesc, string userId, int paymentType = 2)
        {
            if (amount == 0)
            {
                return string.Empty;
            }
            Order order = CreateOrder(amount, "", productDesc, productName, "", "", userId, "", paymentType);
           
            if (order != null && order.OrderNo != null)
            {
                SaveOrder(order);
                return order.OrderNo;
            }
            else
            {
                return string.Empty;
            }
        }

        public void AlipaySDKCallback(XmlDocument xmlDoc, string callBackUrl)
        {
            #region Prepare parameters
            string partner = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/partner");
            string discount = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/discount");
            string payment_type = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/payment_type");
            string subject = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/subject");
            string trade_no = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/trade_no");
            string buyer_email = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/buyer_email");
            string gmt_create = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/gmt_create");
            string quantity = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/quantity");
            string out_trade_no = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/out_trade_no");
            string seller_id = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/seller_id");
            string trade_status = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/trade_status");
            string total_fee = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/total_fee");
            string gmt_payment = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/gmt_payment");
            string seller_email = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/seller_email");
            string gmt_close = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/gmt_close");
            string price = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/price");
            string buyer_id = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/buyer_id");
            string use_coupon = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/use_coupon");
            string is_total_fee_adjust = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/is_total_fee_adjust");
            #endregion

           var order = Repository.Single<Order>(o => o.OrderNo == out_trade_no);
            if (order!=null)
            {
                order.Amount = total_fee.ToFloat();
                order.CallBackUrl = callBackUrl;
                order.OrderNo = out_trade_no;
                order.OrderStatus = trade_status.EqualsOrdinalIgnoreCase("TRADE_FINISHED") ? OrderStatus.Successed : OrderStatus.Failed;
                order.PayedAmount = total_fee.ToFloat();
                order.ProductName = subject;
                order.MerchantExtentionalDescription = out_trade_no;
                //order.UserId = buyer_id;
                order.UserName = buyer_email;
                order.CreatedDate = gmt_create.ToCNDate();

                Repository.Update<Order>(order);
            }
                // create order line
                OrderLine orderLine = new OrderLine
                {
                    OrderNo = out_trade_no,
                    RealAmount = total_fee.ToFloat(),
                    PaymentAmount = total_fee.ToFloat(),
                    PaymentStatus = trade_status.EqualsOrdinalIgnoreCase("TRADE_FINISHED") ? "SUCCESS" : "FAILED",
                    Currency = "RMB",
                    ProductName = subject,
                    YeepayPayNo = trade_no,
                    YeepayUId = buyer_id,
                    ConfirmAmount = total_fee.ToFloat(),
                    ChargeType = payment_type,
                    PaymentDate = gmt_close.ToCNDate(),
                    PaymentNoticeTime = gmt_payment.ToCNDate(),
                    Price = price.ToFloat(),
                    Discount = discount.ToFloat(),
                    Quantity = quantity.ToInt32(),
                    IsTotalFeeAdjust = is_total_fee_adjust.EqualsOrdinalIgnoreCase("N") ? false : true,
                    UseCoupon = use_coupon.EqualsOrdinalIgnoreCase("N") ? false : true
                };
                Repository.Add<OrderLine>(orderLine);

                // send call back to client
                //var cardPaymentCallbackResult = new PaymentNotification();
                //cardPaymentCallbackResult.OrderNo = trade_no;
                //cardPaymentCallbackResult.SuccessAmount = total_fee.ToFloat();
                //cardPaymentCallbackResult.RequestAmount = total_fee.ToFloat();
                //cardPaymentCallbackResult.ResultCode = 0;
                //cardPaymentCallbackResult.CallbackURL = callBackUrl;
                //this.RedisService.AddItemToQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE, cardPaymentCallbackResult);


            
        }

        public void AlipayWapCallback(string result, string sign, SortedDictionary<string, string> parameteres)
        {
            string orderNo = parameteres.ContainsKey("out_trade_no") ? parameteres["out_trade_no"] : string.Empty;

            if (!string.IsNullOrEmpty(orderNo))
            {
                var order = Repository.Single<Order>(o => o.OrderNo == orderNo);
                if (order != null && order.OrderStatus == OrderStatus.Created)
                {
                    order.OrderStatus = OrderStatus.Successed;
                    order.PayedAmount = order.Amount;
                    Repository.Update<Order>(order);
                }
            }

        }

        public void AlipayWapNotify(string notify_data)
        {
            #region Prepare parameteres
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(notify_data);
            string subject = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/subject");
            string buyer_email = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/buyer_email");
            string out_trade_no = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/out_trade_no");
            string total_fee = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/total_fee");
            string seller_email = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/seller_email");
            string price = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/price");
            string notify_id = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/notify_id");
            string gmt_payment = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/gmt_payment");
            string gmt_close = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/gmt_close");
            string payment_type = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/payment_type");
            string trade_no = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/trade_no");
            string gmt_create = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/gmt_create");
            string notify_type = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/notify_type");
            string quantity = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/quantity");
            string notify_time = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/notify_time");
            string seller_id = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/seller_id");
            string trade_status = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/trade_status");
            string is_total_fee_adjust = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/is_total_fee_adjust");
            string buyer_id = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/buyer_id");
            string use_coupon = Alipay.Class.Function.GetStrForXmlDoc(xmlDoc, "notify/use_coupon");
            #endregion

            if (!string.IsNullOrEmpty(out_trade_no))
            {
                var order = Repository.Single<Order>(o => o.OrderNo == out_trade_no);
                if (order != null)
                {
                    if (order.OrderStatus == OrderStatus.Created)
                    {
                        order.OrderStatus = OrderStatus.Successed;
                        order.PayedAmount = order.Amount;
                        Repository.Update<Order>(order);
                    }

                    //create order line
                    OrderLine orderLine = new OrderLine
                    {
                        OrderNo = out_trade_no,
                        RealAmount = total_fee.ToFloat(),
                        PaymentAmount = total_fee.ToFloat(),
                        PaymentStatus = trade_status.EqualsOrdinalIgnoreCase("TRADE_FINISHED") ? "SUCCESS" : "FAILED",
                        Currency = "RMB",
                        ProductName = order.ProductName,
                        YeepayPayNo = trade_no,
                        ConfirmAmount = order.Amount,
                        ChargeType = payment_type,
                        PaymentDate = gmt_close.ToCNDate(),
                        PaymentNoticeTime = gmt_payment.ToCNDate(),
                        Price = price.ToFloat(),
                        Quantity = quantity.ToInt32(),
                        IsTotalFeeAdjust = is_total_fee_adjust.EqualsOrdinalIgnoreCase("N") ? false : true,
                        UseCoupon = use_coupon.EqualsOrdinalIgnoreCase("N") ? false : true
                    };
                    Repository.Add<OrderLine>(orderLine);

                    // send call back to client
                    //var cardPaymentCallbackResult = new PaymentNotification();
                    //cardPaymentCallbackResult.OrderNo = out_trade_no;
                    //cardPaymentCallbackResult.SuccessAmount = total_fee.ToFloat();
                    //cardPaymentCallbackResult.RequestAmount = total_fee.ToFloat();
                    //cardPaymentCallbackResult.ResultCode = 0;
                    //cardPaymentCallbackResult.CallbackURL = order.CallBackUrl;
                    //this.RedisService.AddItemToQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE, cardPaymentCallbackResult);
                }
 
            }

            


        }

        public string VerifyAlipayWapNotify(string notify_data, string sign, string service, string v, string sec_id)
        {
            notify_data = Function.Decrypt(notify_data, Config.PrivateKey, Config.Input_charset_UTF8);

            //创建待签名数组，注意Notify这里数组不需要进行排序，请保持以下顺序
            Dictionary<string, string> sArrary = new Dictionary<string, string>();

            //组装验签数组
            sArrary.Add("service", service);
            sArrary.Add("v", v);
            sArrary.Add("sec_id", sec_id);
            sArrary.Add("notify_data", notify_data);

            //把数组所有元素，按照“参数=参数值”的模式用“&”字符拼接成字符串
            string content = Function.CreateLinkString(sArrary);

            //验证签名
            bool vailSign = Function.Verify(content, sign, Config.Alipaypublick, Config.Input_charset_UTF8);
            if (vailSign)
            {
                return notify_data;
            }
            else
            {
                return string.Empty;
            }
        }

        public string InitAlipayWapPay(string productName, string fee, string userId, string clientCallbackUrl)
        {
            //初始化Service
            Alipay.Class.Service ali = new Alipay.Class.Service();

            // create new order
            string orderNo = GenerateOrderNumber();
            Order order = new Order
            {
                Amount = fee.ToFloat(),
                ProductName = productName,
                UserId = userId,
                OrderStatus = OrderStatus.Created,
                CreatedDate = DateTime.Now,
                CallBackUrl = clientCallbackUrl,
                OrderNo = orderNo,
                Currency = "CNY",
            };
            Repository.Add<Order>(order);

            //创建交易接口
            string token = ali.alipay_wap_trade_create_direct(productName, orderNo, fee, Config.Seller_account_name, Config.Notify_url,
               userId, Config.Merchant_url, Config.Call_back_url, Config.Service_Create, Config.Sec_id, Config.Partner, orderNo, 
               Config.Format, Config.V, Config.Req_url, Config.PrivateKey, Config.Input_charset_UTF8);

            //构造，重定向URL
            string url = ali.alipay_Wap_Auth_AuthAndExecute(Config.Req_url, Config.Sec_id, Config.Partner, Config.Call_back_url, 
                Config.Format, Config.V, Config.Service_Auth, token, Config.Req_url, Config.PrivateKey, Config.Input_charset_UTF8);

            return url;
        }

   
        #endregion


        #region helper

        private void SuccessfullyCallBackLogic(BuyCallbackResult result)
        {
            OrderLine orderLine = new OrderLine();
            orderLine.ChargeType = result.R0_Cmd;
            orderLine.PaymentStatus = result.R1_Code.Equals("1") ? "SUCCESS" : "FAILED";
            orderLine.YeepayPayNo = result.R2_TrxId;
            orderLine.PaymentAmount = float.Parse(result.R3_Amt);
            orderLine.Currency = result.R4_Cur;
            orderLine.ProductName = result.R5_Pid;
            orderLine.OrderNo = result.R6_Order;
            orderLine.YeepayUId = result.R7_Uid;
            if (string.IsNullOrEmpty(result.Rp_PayDate))
            {
                orderLine.PaymentDate = DateTime.ParseExact(result.Rp_PayDate, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
            }

            YeepayCallBackTransaction(orderLine);
        }

        internal void CardCallBackLogic(SZXCallbackResult result)
        {
            string callBackUrl = string.Empty;
            this.UpdateOrderStatus(result, out callBackUrl);

            //persistent
            this.AddOrderLine(result);
        }

        private void AddNotifyQueueToQ2(SZXCallbackResult result, string callBackUrl)
        {
            //add order to Q2
            var cardPaymentCallbackResult = new PaymentNotification();
            cardPaymentCallbackResult.OrderNo = result.P2_Order;
            cardPaymentCallbackResult.SuccessAmount = result.P3_Amt.ToFloat();
            cardPaymentCallbackResult.RequestAmount = Repository.Single<Order>(s => s.OrderNo == result.P2_Order).Amount;//result.P6_confirmAmount.ToFloat();
            cardPaymentCallbackResult.ResultCode = result.P8_cardStatus.ToInt32();
            cardPaymentCallbackResult.CallbackURL = callBackUrl;

            var desc = string.Empty;
            CardPaymentDataDict.CardPaymentCallBackStatusDesc.TryGetValue(result.P8_cardStatus, out desc);
            cardPaymentCallbackResult.Description = desc;

            this.RedisService.AddItemToQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE, cardPaymentCallbackResult);
        }

        internal void BankCardCallBack(SDKCallbackResult result)
        {

            var order = Repository.Single<Order>(s => s.OrderNo == result.R6_Order);
            if (order.OrderStatus!=OrderStatus.Successed)
            {
                order.OrderStatus= OrderStatus.Successed;
                order.PayedAmount = result.R3_Amt.ToFloat();
                Repository.Update<Order>(order);

                DateTime DEFAULT_TIME_PAYMENT , DEFAULT_TIME_NOTIFY = DateTime.Now;
                DateTime.TryParse(result.Rp_PayDate, out DEFAULT_TIME_PAYMENT);//Convert.ToDateTime(result.Rp_PayDate),
                DateTime.TryParse(result.Ru_Trxtime, out DEFAULT_TIME_NOTIFY);

                OrderLine orderLine = new OrderLine
                    {
                        ChargeType = result.R0_Cmd,
                        PaymentStatus = result.R1_Code.Equals("1") ? "SUCCESS" : "Faild",
                        BatchNo = result.R2_TrxId,
                        YeepayPayNo = result.R4_Cur + result.Rq_CardNo,
                        PaymentAmount = result.R3_Amt.ToFloat(),
                        Currency = result.R4_Cur,
                        ProductName = result.R5_Pid,
                        OrderNo = result.R6_Order,
                        YeepayUId = result.R7_Uid,
                        ExpandInfo = result.R8_MP,
                        CallbackType = result.R9_BType.ToInt32(),
                        ChannelNo = result.Rb_BankId,
                        BankOrderId = result.Ro_BankOrderId,
                        PaymentDate = DEFAULT_TIME_PAYMENT,
                        CardNo = result.Rq_CardNo,
                        NotifyDate = DEFAULT_TIME_NOTIFY,
                        MerchantCode = result.P1_MerId
                    };
                this.Repository.Add<OrderLine>(orderLine);
            }
        }

        //internal void CallBackLogic_SDK(SDKCallbackResult result)
        //{
        //    string callBackUrl = string.Empty;

        //    Order order = new Order
        //    {
        //        OrderNo = result.R6_Order,
        //        Amount = result.R3_Amt.ToFloat(),
        //        Currency = "RMB",
        //        ProductName = result.R5_Pid,
        //        MerchantExtentionalDescription = result.R8_MP
        //    };

        //    order.Status = result.R1_Code.Equals("1") ? OrderStatus.Successed : OrderStatus.Failed;

        //    var oldOrder = this.Repository.Single<Order>(m => m.OrderNo == result.R6_Order);

        //    lock (sync)
        //    {
        //        if (oldOrder == null)
        //        {
        //            this.Repository.Add<Order>(order);
                    
        //            var cardPaymentCallbackResult = new PaymentNotification();
        //            cardPaymentCallbackResult.OrderNo = result.R6_Order;
        //            cardPaymentCallbackResult.SuccessAmount = result.R3_Amt.ToFloat();
        //            cardPaymentCallbackResult.RequestAmount = result.R3_Amt.ToFloat();
        //            cardPaymentCallbackResult.ResultCode = result.R1_Code.Equals("1") ? ((int)OrderStatus.Successed) : ((int)OrderStatus.Failed);
        //            cardPaymentCallbackResult.CallbackURL = callBackUrl;

        //            this.RedisService.AddItemToQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE, cardPaymentCallbackResult);

        //            OrderLine orderLine = new OrderLine
        //            {
        //                ChargeType = result.R0_Cmd,
        //                PaymentStatus = result.R1_Code.Equals("1") ? "SUCCESS" : "Faild",
        //                BatchNo = result.R2_TrxId,
        //                YeepayPayNo = result.R4_Cur + result.Rq_CardNo,
        //                PaymentAmount = result.R3_Amt.ToFloat(),
        //                Currency = result.R4_Cur,
        //                ProductName = result.R5_Pid,
        //                OrderNo = result.R6_Order,
        //                YeepayUId = result.R7_Uid,
        //                ExpandInfo = result.R8_MP,
        //                CallbackType = result.R9_BType.ToInt32(),
        //                ChannelNo = result.Rb_BankId,
        //                BankOrderId = result.Ro_BankOrderId,
        //                PaymentDate = Convert.ToDateTime(result.Rp_PayDate),
        //                CardNo = result.Rq_CardNo,
        //                NotifyDate = Convert.ToDateTime(result.Ru_Trxtime),
        //                MerchantCode = result.P1_MerId
        //            };

        //            this.Repository.Add<OrderLine>(orderLine);
        //        }
        //    }
        //}

        internal void AddOrderLine(SZXCallbackResult result)
        {
            OrderLine orderLine = new OrderLine();

            orderLine.ChargeType = result.R0_Cmd;
            orderLine.PaymentStatus = result.R1_Code.Equals("1") ? "SUCCESS" : "FAILED";
            orderLine.YeepayPayNo = result.P4_FrpId + result.P5_CardNo;//神州行卡充值无唯一充值标识
            orderLine.PaymentAmount = result.P3_Amt.ToFloat();
            orderLine.Currency = result.P4_FrpId;
            orderLine.OrderNo = result.P2_Order;
            orderLine.CardNo = result.P5_CardNo;
            orderLine.RealAmount = result.P7_realAmount.ToFloat();
            orderLine.ConfirmAmount = result.P6_confirmAmount.ToFloat();
            orderLine.CardStatus = (CardPaymentCallBackStatus)result.P8_cardStatus.ToInt32();

            orderLine.MerchantCode = result.P1_MerId;
            orderLine.ExpandInfo = result.P9_MP;

            if (!string.IsNullOrEmpty(result.Pc_BalanceAct))
            {
                //支付成功，且需要订单校验时有值，
                orderLine.BalanceAmount = result.Pb_BalanceAmt.ToFloat();
                orderLine.BalanceAct = result.Pc_BalanceAct;
            }

            this.Repository.Add<OrderLine>(orderLine);
        }

        internal bool UpdateOrderStatus(SZXCallbackResult result, out string callBackUrl)
        {
            var isUpdated = false;
            callBackUrl = string.Empty;

            var order = this.Repository.Single<Order>(m => m.OrderNo == result.P2_Order);

            if (order != null && order.OrderStatus == OrderStatus.Processing)
            {
                order.PayedAmount = float.Parse(result.P3_Amt);
                callBackUrl = order.CallBackUrl;

                var isCarStatusExist = Enum.IsDefined(typeof(CardPaymentCallBackStatus), result.P8_cardStatus.ToInt32());
                if (isCarStatusExist)
                {
                    order.CardPaymentCallBackStatus = (CardPaymentCallBackStatus)result.P8_cardStatus.ToInt32();
                }
                else
                {
                    order.CardPaymentCallBackStatus = CardPaymentCallBackStatus.UnKnownedError;
                }

                if (result.R1_Code.Equals("1"))
                {
                    order.OrderStatus = OrderStatus.Successed;
                }
                else
                {
                    order.OrderStatus = OrderStatus.Failed;
                }
                this.Repository.Update<Order>(order);
            }
            else
            {
                isUpdated = true;
            }
            

            return isUpdated;
        }


        public Order CreateOrder(float Amount, string Desription, string productDescription, string productName, string productType, string callBackUrl, string userId, string userName, int paymentType, int? cardType = null, string cardNo = "", string cardPassword = "", float? cardAmount = null)
        {
            Order order = new Order();
            if (Amount <= 0)
            {
                return order;
            }

            order.Amount = Amount;
            order.CreatedDate = DateTime.Now;
            order.MerchantExtentionalDescription = Desription.MakeSureUnicodeStringByteLength(200);
            order.CardPaymentCallBackStatus = CardPaymentCallBackStatus.NotCallBack;
            order.CardPaymentRequestStatus = CardPaymentRequestStatus.NotRequest;
            order.PayedAmount = 0;
            order.ProductDescription = productDescription.MakeSureUnicodeStringByteLength(30);
            order.ProductName = productName.MakeSureUnicodeStringByteLength(30);
            order.ProductType = productType.MakeSureUnicodeStringByteLength(30);
            order.OrderNo = GenerateOrderNumber();
            order.CallBackUrl = callBackUrl;
            order.OrderStatus = OrderStatus.Created;
            order.UserId = userId;
            order.UserName = userName;

            order.PaymentType = (PaymentType)paymentType;
            order.CardType = cardType;
            order.CardNo = cardNo;
            order.CardPassword = cardPassword;
            order.CardAmount = cardAmount;
            return order;
        }

        internal long SaveOrder(Order order)
        {
            return Repository.NewAdd<Order>(order);
        }

        internal CardPayment CreateCardPayment(Order order, int cardType, string cardNo, string cardPassword, float cardAmount, float Amount, string productName, string productType, string productDescription, string MerchantExtentionalDescription)
        {
            CardPayment cardPayment = new CardPayment();
            if (cardNo.IsNullOrEmpty() || cardPassword.IsNullOrEmpty())
            {
                return cardPayment;
            }
            cardPayment.Amount = Amount;
            cardPayment.CardAmount = cardAmount;
            cardPayment.CardNo = cardNo;
            cardPayment.CardPassword = cardPassword;
            cardPayment.CardType = (PaymentCardType)cardType;
            cardPayment.MerchantExtentionalDescription = MerchantExtentionalDescription;
            cardPayment.OrderNo = order.OrderNo;
            cardPayment.ProductDescription = productDescription;
            cardPayment.ProductName = productName;
            cardPayment.ProductType = productType;
            cardPayment.OrderID = order.Id;

            return cardPayment;
        }

        internal void SaveCardPayment(CardPayment cardPayment)
        {
            RedisService.AddItemToQueue<CardPayment>(BillingConsts.KEY_CARD_PAYMENT_REQUEST_PROCESSING_QUEUE, cardPayment);
        }


        internal string GenerateOrderNumber()
        {
            var orderCount = 0;
            int maxCount = 10000000;
            lock (this.sync)
            {
                orderCount = (int)(RedisService.GetNextSequenceNum(SEQUENCE_ORDER_COUNT) % maxCount);
                return string.Format("{0}{1}", DateTime.Now.ToString(DateTimeFormat.yyyyMMddHHmmss), orderCount.ToString().PadLeft(7, '0'));
            }
        }

        internal string ConvertUTF8ToGBK(string instring)
        {
            if (string.IsNullOrEmpty(instring))
            {
                return "";
            }
            return Encoding.GetEncoding("gb2312").GetString(Encoding.GetEncoding("UTF-8").GetBytes(instring));
        }
        #endregion

    }
}
