using System;
using com.yeepay.cmbn;
using System.Collections.Generic;
using BlackMamba.Billing.Models.Payments;
using BlackMamba.Billing.Domain.ViewModels.Billing;
using BlackMamba.Billing.Models;

namespace BlackMamba.Billing.Domain.Services
{
    public interface IMailService
    {
        //string GetYeepayPaymentUrlByOrder(string orderNo);

        void YeepayCallBackTransaction(OrderLine orderLine);

        string ReissueOrder(string orderNo);

        string RefundMoney(string yeepayPayNo, string RefundDesc);

        PaymentDetails YeepayCallBack(string p1_merid, string r0_cmd, string r1_code, string r2_trxid, string r3_amt, string r4_cur, string r5_pid, string r6_order, string r7_uid, string r8_mp, string r9_btype, string rp_paydate, string hmac);

        SZXResult YeepayCardPartialPayments(CardPayment cardPayment);
        SZXResult YeepayCardPayments(CardPayment cardPayment);
        string YeepayCardCallBack(string r0_Cmd, string r1_Code, string p1_MerId, string p2_Order, string p3_Amt, string p4_FrpId, string p5_CardNo, string p6_confirmAmount, string p7_realAmount, string p8_cardStatus, string p9_MP, string pb_BalanceAmt, string pc_BalanceAct, string hmac);
        
        string CheckPaymentStatus(string orderNo);

        OrderViewModel RequestCardPayment(int cardType, string cardNo, string cardPassword, float cardAmount, float Amount, string productName, string productType, string productDescription, string MerchantExtentionalDescription, string callBackUrl, string userId, string userName);

        Order RequestBankCardPayment(float Amount, string productName, string productType, string productDescription, string MerchantExtentionalDescription, string callBackUrl, string userId, string userName);
        void AlipaySDKCallback(System.Xml.XmlDocument xmlDoc, string callBackUrl);

        //string YeepaySDKCallBack(string p1_MerId, string r0_Cmd, string r1_Code, string r2_TrxId, string r3_Amt, string r4_Cur, string r5_Pid, string r6_Order, string r7_Uid, string r8_MP, string r9_BType, string rb_BankId, string ro_BankOrderId, string rp_PayDate, string rq_CardNo, string ru_Trxtime, string hmac);

        string YeepayBankCardCallBack(string p1_MerId, string r0_Cmd, string r1_Code, string r2_TrxId, string r3_Amt, string r4_Cur, string r5_Pid, string r6_Order, string r7_Uid, string r8_MP, string r9_BType, string rb_BankId, string ro_BankOrderId, string rp_PayDate, string rq_CardNo, string ru_Trxtime, string hmac);

        string CreateOrder(string productName, float amount, string productDesc, string userId, int paymentType = 2);

        string InitAlipayWapPay(string productName, string fee, string userId, string clientCallbackUrl);

        void AlipayWapCallback(string result, string sign, SortedDictionary<string, string> parameteres);

        void AlipayWapNotify(string notify_data);

        string VerifyAlipayWapNotify(string notify_data, string sign, string service, string v, string sec_id);

        Order CreateOrder(float Amount, string Desription, string productDescription, string productName, string productType, string callBackUrl, string userId, string userName, int paymentType, int? cardType = null, string cardNo = "", string cardPassword = "", float? cardAmount = null);
    }
}
