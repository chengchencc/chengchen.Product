using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Models.Payments
{
    public enum CardPaymentRequestStatus
    {
        NotRequest = 0, // initial status

        RequestFailed = -2, // 请求第三方支付没有返回结果

        Success = 1, //“1”代表提交成功，非“1”代表提交失败

        SignatureError = -1, //签名较验失败或未知错误

        UseCardTooFrequency = 2, // 卡密成功处理过或者提交卡号过于频繁

        CardNumberExceed = 5,// 卡数量过多，目前最多支持10张卡

        DuplicateOrderNo = 11,// 订单号重复

        PayAmountError = 66,// 支付金额有误

        PayMethodNotAvaliable = 95,// 支付方式未开通

        CardNotSupported = 112,// 业务状态不可用，未开通此类卡业务

        CardAmoutError = 8001,// 卡面额组填写错误

        CardPasswordError = 8002// 卡号密码为空或者数量不相等（使用组合支付时）
    }
}
