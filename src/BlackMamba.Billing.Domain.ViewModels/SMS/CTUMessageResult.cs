using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Domain.ViewModels.SMS
{
    public enum CTUMessageResultCodes
    {
        /// <summary>
        /// 1  = 发送成功
        /// </summary>
        Success = 0,
        /// <summary>
        /// -1 = 用户名和密码参数为空或者参数含有非法字符
        /// </summary>
        NullArgumentOrInvalidCharacters = 1,
        /// <summary>
        /// -2 = 手机号参数不正确
        /// </summary>
        InvalidPhoneNumber = 2,
        /// <summary>
        /// -3 = msg参数为空或长度小于0个字符
        /// </summary>
        InvalidMessage = 3,
        /// <summary>
        /// -4 = msg参数长度超过64个字符
        /// </summary>
        MessageLengthIsTooLong = 4,
        /// <summary>
        /// -6 = 发送号码为黑名单用户
        /// </summary>
        TargetUserIsOnBlacklist = 6,
        /// <summary>
        /// -8 = 下发内容中含有屏蔽词
        /// </summary>
        ContainsShieldWord = 8,
        /// <summary>
        /// -9 = 下发账户不存在
        /// </summary>
        AccountNotExist = 9,
        /// <summary>
        /// -10 = 下发账户已经停用
        /// </summary>
        AccountOutOfService = 10,
        /// <summary>
        /// -11 = 下发账户无余额
        /// </summary>
        AccountWithNoBalance = 11,
        /// <summary>
        /// -15 = MD5校验错误
        /// </summary>
        MD5VarifyFailed = 15,
        /// <summary>
        /// -16 = IP服务器鉴权错误
        /// </summary>
        ServerIPAuthenticationFailed = 16,
        /// <summary>
        /// -17 = 接口类型错误
        /// </summary>
        InterfaceTypeError = 17,
        /// <summary>
        /// -18 = 服务类型错误
        /// </summary>
        ServiceTypeError = 18,
        /// <summary>
        /// -22 = 手机号达到当天发送限制
        /// </summary>
        PhoneNumberArchievedLimitation = 22,
        /// <summary>
        /// -23 = 同一手机号，相同内容达到当天发送限制
        /// </summary>
        PhoneNumberArchievedLimitationWithSameContent = 23,
        /// <summary>
        /// -99 = 系统异常
        /// </summary>
        SystemError = 99

    }
    public class CTUMessageResult : ViewModelBase
    {
        public CTUMessageResultCodes Result { get; set; }

        public override string ToString()
        {
            var code = 1;

            if (Result != CTUMessageResultCodes.Success)
            {
                code = (-1) * (int)Result;
            }

            return string.Format("<Result>{0}</Result>", code);
        }
    }
}
