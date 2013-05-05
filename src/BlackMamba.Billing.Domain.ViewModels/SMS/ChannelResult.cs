using BlackMamba.Billing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Domain.ViewModels.SMS
{
    public enum CTURequestStatus
    {
        UnknownError = -1,
        Success= 0,//表示提交成功，可以直接读取Msg值返回提示给用户
        ParameterError = 1,//表示提交参数不完整，Msg为空
        IpError= 2,//表示提交ip不合法，Msg为空
        KeyError= 3,//表示提交key错误，Msg为空
        ProduectIdError= 4,//表示提交Productsid错误，Msg为空
        AmountError = 5//表示提交直充金额不对，Msg为空
    }
    public enum ChannelRequestStatus
    {
        Success= 0,
        MissingMobileInfo,
        NoChannelFound
    }

    public class ChannelResult : ViewModelBase
    {
        public string ServiceNumber { get; set; }

        public string Code { get; set; }

        public string OrderNo { get; set; }

        public long LogId { get; set; }

        public CTURequestStatus CTURequestStatus { get; set; }

        public ChannelRequestStatus Status { get; set; } 

        public SMSChannelSetting SMSChannelSetting { get; set; }

    }
}
