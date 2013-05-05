using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Models.Payments
{
    public enum CardPaymentCallBackStatus
    {
        NotCallBack = -1, // initial status
        RequestError = -2, // all error statuses in CardPaymentRequestStatus

        Success = 0,       // 销卡成功，订单成功
        Failure = 1,       // 销卡成功，订单失败
        CardError = 7,       // 卡号卡密或卡面额不符合规则
        SubmitTooFrequency = 1002,       // 本张卡密您提交过于频繁，请您稍后再试
        CardNotSupported = 1003,       // 不支持的卡类型（比如电信地方卡）
        CardPasswordError = 1004,       // 密码错误或充值卡无效
        CardNotValid = 1006,       // 充值卡无效
        CardAmountNotEnough = 1007,       // 卡内余额不足
        CardExpired = 1008,       // 余额卡过期（有效期1个月）
        CardProcessing = 1010,       // 此卡正在处理中
        UnKnownedError = 10000,       // 未知错误
        CardUsed = 2005,       // 此卡已使用
        CardInfoProcessing = 2006,       // 卡密在系统处理中
        FakeCard = 2007,       // 该卡为假卡
        CardMaintaining = 2008,       // 该卡种正在维护
        ZheJiangMaintaining = 2009,       // 浙江省移动维护
        JiangSuMaintaining = 2010,       // 江苏省移动维护
        FuJianMaintaining = 2011,       // 福建省移动维护
        LiaoNingMaintaining = 2012,       // 辽宁省移动维护
        CardLocked = 2013,       // 该卡已被锁定
        SystemBusy = 2014,       // 系统繁忙，请稍后再试

        //下面为易宝e卡通返回的错误代码
        YPCardNotAvaliable = 3001,       // 卡不存在
        YPCardAbolished = 3002,       // 卡已使用过
        YPCard = 3003,       // 卡已作废
        YPFrozen = 3004,       // 卡已冻结
        YPNotActivated = 3005,       // 卡未激活
        YPPasswordError = 3006,       // 密码不正确
        YPCardProcessing = 3007,       // 卡正在处理中
        YPSystemError = 3101,       // 系统错误
        YPCardExpired = 3102       // 卡已过期
    }
}
