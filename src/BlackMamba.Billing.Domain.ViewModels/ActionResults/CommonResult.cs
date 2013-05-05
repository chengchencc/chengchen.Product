using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Domain.ViewModels
{
    public class CommonResult
    {
        internal const string SUCCESSFUL = "成功";
        internal const string USER_NAME_IS_EMPTY = "注册用户名为空";
        internal const string INVALID_CHARGE_CARD = "充值卡无效";
        internal const string BALANCE_IS_NOT_ENOUGH = "余额不足";
        internal const string ORDER_NOT_EXIST = "订单不存在";
        internal const string USER_NOT_EXIST = "用户不存在";
        internal const string PHONE_ALREADY_USED = "该手机号已被使用";
        internal const string PHONE_ALREADY_BOUND = "用户已绑定该手机号";
        internal const string NO_RECORD = "记录不存在";
        internal const string NO_RECORD_FOUND = "未找到符合项";
        internal const string PROCESS_TIMEOUT = "处理超时";
        internal const string AMOUNT_INVALID = "金额无效";
        internal const string RQUEST_OUT_OF_DATE = "请求已过期";
        internal const string PARAM_ERROR = "参数错误";
        internal const string ENCRYPTION_SIGN_INVALID = "加密签名无效";
        internal const string INTERNAL_ERROR = "系统错误";

        public static Dictionary<int, string> ResultDic
        {
            get
            {
                if (s_resultDic == null)
                {
                    s_resultDic = new Dictionary<int, string>();
                    s_resultDic[0] = SUCCESSFUL;
                    s_resultDic[1] = USER_NAME_IS_EMPTY;
                    s_resultDic[2] = INVALID_CHARGE_CARD;
                    s_resultDic[3] = BALANCE_IS_NOT_ENOUGH;
                    s_resultDic[4] = ORDER_NOT_EXIST;
                    s_resultDic[5] = USER_NOT_EXIST;
                    s_resultDic[6] = PHONE_ALREADY_USED;
                    s_resultDic[7] = PHONE_ALREADY_BOUND;
                    s_resultDic[8] = NO_RECORD;
                    s_resultDic[993] = NO_RECORD_FOUND;
                    s_resultDic[994] = PROCESS_TIMEOUT;
                    s_resultDic[995] = AMOUNT_INVALID;
                    s_resultDic[996] = RQUEST_OUT_OF_DATE;
                    s_resultDic[997] = PARAM_ERROR;
                    s_resultDic[998] = ENCRYPTION_SIGN_INVALID;
                    s_resultDic[999] = INTERNAL_ERROR;
                }
                return s_resultDic;
            }
        } private static Dictionary<int, string> s_resultDic;


        public CommonResult()
        {
            this.Result = 999;
        }

        public int Result
        {
            get
            {
                return _result;
            }
            set
            {
                Desc = GetDescriptionByResult(value);
                _result = value;
            }
        }   private int _result;

        public string Desc { get; set; }

        public static string GetDescriptionByResult(int code)
        {
            var result = string.Empty;
            if (ResultDic.ContainsKey(code))
                result = ResultDic[code];

            return result;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("result={0},", Result);
            sb.AppendFormat("desc={0},", Desc);

            return sb.ToString();
        }
    }
}
