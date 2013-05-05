using System;
using System.Collections.Generic;
using System.Linq;
using BlackMamba.Framework.Core;
using System.Text;
using com.yeepay.cmbn;
using com.yeepay.Utils;


using System.Configuration;
using BlackMamba.Billing.Models.Billing;
namespace BlackMamba.Billing.Domain.Services
{
    public class YeepayService: IYeepayService
    {
        public SZXCallbackResult VerifyCallback(string r0_Cmd, string r1_Code, string p1_MerId, string p2_Order, string p3_Amt, string p4_FrpId, string p5_CardNo, string p6_confirmAmount, string p7_realAmount, string p8_cardStatus, string p9_MP, string pb_BalanceAmt, string pc_BalanceAct, string hmac)
        {
            return SZX.VerifyCallback(r0_Cmd, r1_Code, p1_MerId, p2_Order, p3_Amt, p4_FrpId, p5_CardNo, p6_confirmAmount, p7_realAmount, p8_cardStatus, p9_MP, pb_BalanceAmt, pc_BalanceAct, hmac);
        }

        public SZXResult AnnulCard(string p2_Order, string p3_Amt, string p4_verifyAmt, string p5_Pid, string p6_Pcat, string p7_Pdesc, string p8_Url,
                    string pa_MP, string pa7_cardAmt, string pa8_cardNo, string pa9_cardPwd, string pd_FrpId, string pr_NeedResponse, string pz_userId, string pz1_userRegTime)
        {
            return SZX.AnnulCard(p2_Order, p3_Amt, p4_verifyAmt, p5_Pid, p6_Pcat, p7_Pdesc, p8_Url,
                    pa_MP, pa7_cardAmt, pa8_cardNo, pa9_cardPwd, pd_FrpId, pr_NeedResponse, pz_userId, pz1_userRegTime);
        }

        public SDKCallbackResult VerifySDKCallBack(string p1_MerId, string r0_Cmd, string r1_Code, string r2_TrxId, string r3_Amt, string r4_Cur, string r5_Pid, string r6_Order, string r7_Uid, string r8_MP, string r9_BType, string rb_BankId, string ro_BankOrderId, string rp_PayDate, string rq_CardNo, string ru_Trxtime, string hmac)
        {
            //ing ro_BankOrderId, string rp_PayDate, string rq_CardNo, string ru_Trxtime, string hmac)
            StringBuilder sb = new StringBuilder();
            sb.Append(p1_MerId)
                .Append(r0_Cmd)
                .Append(r1_Code)
                .Append(r2_TrxId)
                .Append(r3_Amt)
                .Append(r4_Cur)
                .Append(r5_Pid)
                .Append(r6_Order)
                .Append(r7_Uid)
                .Append(r8_MP)
                .Append(r9_BType);

            var verifiedStr = Digest.HmacSign(sb.ToString(), "keyValue".ConfigValue());
            if (verifiedStr.Equals(hmac))
            {
                return new SDKCallbackResult(p1_MerId, r0_Cmd, r1_Code, r2_TrxId, r3_Amt, r4_Cur, r5_Pid, r6_Order, r7_Uid, r8_MP, r9_BType, rb_BankId, ro_BankOrderId, rp_PayDate, rq_CardNo, ru_Trxtime, hmac, "");
            }

            return new SDKCallbackResult(p1_MerId, r0_Cmd, r1_Code, r2_TrxId, r3_Amt, r4_Cur, r5_Pid, r6_Order, r7_Uid, r8_MP, r9_BType, rb_BankId, ro_BankOrderId, rp_PayDate, rq_CardNo, ru_Trxtime, hmac,  verifiedStr + "<br>sbOld:" + sb.ToString()); 
        }
    }
}
