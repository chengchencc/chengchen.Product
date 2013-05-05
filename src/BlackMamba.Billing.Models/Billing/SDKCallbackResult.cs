using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Models.Billing
{
    public class SDKCallbackResult
    {
        public string P1_MerId;
        public string R0_Cmd;
        public string R1_Code;
        public string R2_TrxId;
        public string R3_Amt;
        public string R4_Cur;
        public string R5_Pid;
        public string R6_Order;
        public string R7_Uid;
        public string R8_MP;
        public string R9_BType;
        public string Rb_BankId;
        public string Ro_BankOrderId;
        public string Rp_PayDate;
        public string Rq_CardNo;
        public string Ru_Trxtime;

        public string Hmac;
        public string ErrMsg;

        public SDKCallbackResult(string p1_MerId, string r0_Cmd, string r1_Code, string r2_TrxId, string r3_Amt, string r4_Cur, string r5_Pid, string r6_Order, string r7_Uid, string r8_MP, string r9_BType, string rb_BankId, string ro_BankOrderId, string rp_PayDate, string rq_CardNo, string ru_Trxtime, string hmac, string errMsg)
        {
            this.P1_MerId = p1_MerId;
            this.R0_Cmd = r0_Cmd;
            this.R1_Code = r1_Code;
            this.R2_TrxId = r2_TrxId;
            this.R3_Amt = r3_Amt;
            this.R4_Cur = r4_Cur;
            this.R5_Pid = r5_Pid;
            this.R6_Order = r6_Order;
            this.R7_Uid = r7_Uid;
            this.R8_MP = r8_MP;
            this.R9_BType = r9_BType;
            this.Rb_BankId = rb_BankId;
            this.Ro_BankOrderId = ro_BankOrderId;
            this.Rp_PayDate = rp_PayDate;
            this.Rq_CardNo = rq_CardNo;
            this.Ru_Trxtime = ru_Trxtime;
            this.Hmac = hmac;
            this.ErrMsg = errMsg;
        }

    }
}
