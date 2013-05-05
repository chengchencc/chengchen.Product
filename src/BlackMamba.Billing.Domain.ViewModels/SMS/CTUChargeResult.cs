using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Domain.ViewModels.SMS
{
    public enum CTUChargeResultCodes
    {
        /// <summary>
        /// 表示提交成功，可以直接读取Msg值返回提示给用户
        /// </summary>
        Success = 0,

        /// <summary>
        /// 表示提交参数不完整，Msg为空
        /// </summary>
        MissingRequiredParam = 1,

        /// <summary>
        /// 表示提交ip不合法，Msg为空
        /// </summary>
        InvalidIPAddress = 2,

        /// <summary>
        /// 表示提交key错误，Msg为空
        /// </summary>
        InvalidKey = 3,

        /// <summary>
        /// 表示提交Productsid错误，Msg为空
        /// </summary>
        InvalidProductSid = 4,

        /// <summary>
        /// 表示提交直充金额不对，Msg为空
        /// </summary>
        InvalidChargeAmount = 5,

        /// <summary>
        /// System Error
        /// </summary>
        InternalException = 999
    }

    public class CTUChargeResult : ViewModelBase
    {
        public CTUChargeResultCodes Result { get; set; }

        public override string ToViewModelString()
        {
            return string.Format("<Result>{0}</Result>", (int)this.Result);
        }
    }
}
