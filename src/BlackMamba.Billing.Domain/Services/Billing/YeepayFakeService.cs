using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.yeepay.cmbn;
using BlackMamba.Billing.Models.Billing;

namespace BlackMamba.Billing.Domain.Services
{
    public class YeepayFakeService : IYeepayService
    {
        public SZXCallbackResult VerifyCallback(string r0_Cmd, string r1_Code, string p1_MerId, string p2_Order, string p3_Amt, string p4_FrpId, string p5_CardNo, string p6_confirmAmount, string p7_realAmount, string p8_cardStatus, string p9_MP, string pb_BalanceAmt, string pc_BalanceAct, string hmac)
        {
            return new SZXCallbackResult(r0_Cmd, r1_Code, p1_MerId, p2_Order, p3_Amt, p4_FrpId, p5_CardNo, p6_confirmAmount, p7_realAmount, p8_cardStatus, p9_MP, pb_BalanceAmt, pc_BalanceAct, hmac, string.IsNullOrEmpty(hmac) ?  "Error message from yeepay" : string.Empty);
        }

        public SDKCallbackResult VerifySDKCallBack(string p1_MerId, string r0_Cmd, string r1_Code, string r2_TrxId, string r3_Amt, string r4_Cur, string r5_Pid, string r6_Order, string r7_Uid, string r8_MP, string r9_BType, string rb_BankId, string ro_BankOrderId, string rp_PayDate, string rq_CardNo, string ru_Trxtime, string hmac)
        {
            return new SDKCallbackResult(p1_MerId, r0_Cmd, r1_Code, r2_TrxId, r3_Amt, r4_Cur, r5_Pid, r6_Order, r7_Uid, r8_MP, r9_BType, rb_BankId, ro_BankOrderId, rp_PayDate, rq_CardNo, ru_Trxtime, hmac, "");
        }

        public SZXResult AnnulCard(string p2_Order, string p3_Amt, string p4_verifyAmt, string p5_Pid, string p6_Pcat, string p7_Pdesc, string p8_Url,
                    string pa_MP, string pa7_cardAmt, string pa8_cardNo, string pa9_cardPwd, string pd_FrpId, string pr_NeedResponse, string pz_userId, string pz1_userRegTime)
        {
            if (pd_FrpId == "999")
            {
                return null;
            }
            else
            {
                return new SZXResult("ChargeCardDirect", pa8_cardNo.Equals("999") ? "-1" : "1", p2_Order, pa8_cardNo.Equals("999") ? "request failed" : string.Empty, string.Empty, p8_Url, "reqResult");
            }
        }
    }
}
