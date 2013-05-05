using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using BlackMamba.Framework.Core;
using NLog;
using Alipay.Class;
using System.Text;
using BlackMamba.Billing.Domain.Services;
using BlackMamba.Billing.Domain.Common;
using BlackMamba.Billing.Models;
using BlackMamba.Billing.Domain.Services.SMS;
using BlackMamba.Billing.Models.SMS;
using BlackMamba.Billing.Domain.ViewModels.SMS;

namespace BlackMamba.Billing.Website.Controllers
{

    public class SMSController : MvcControllerBase
    {
        protected ISMSService SMSService { get; set; }
        public SMSController(ISMSService smsService)
        {
            SMSService = smsService;
        }

        /// <summary>
        /// Query the instruction & service number from server.
        /// </summary>
        /// <param name="imsi"></param>
        /// <param name="mobile"></param>
        /// <returns>Server should return instruction + service number , order number and Channel Log ID to client.</returns>
        public ActionResult QueryChannel(string imsi, string mobile, ServiceType serviceType, float? amount, string userNo)
        {
            var actionResult =
                BuildResult<ChannelResult>(() => true, () => SMSService.QueryChannel(imsi, mobile, serviceType, amount, userNo), false);

            return Content(actionResult.ToString());
        }

        /// <summary>
        /// Client sms log for the order checking, recommend to use http post method.
        /// 
        /// Two function:
        /// 1. sync sms content
        /// 2. update the sms charge order status (initial -> sent; or sent -> confirmed)
        /// 
        /// After sending the sms, notify the server and change the order status to sent.
        /// After sending the confirm sms, notify the server and change the order status to confirmed.
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="orderStatus"></param>
        /// <param name="content"></param>
        /// <param name="smsDirection"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SMSLog(long logId, SMSChargeStatus chargeStatus, string content, SMSDirection smsDirection, bool? isSent, string targetPhoneNo,string orderNo,string outOrderNo,string partenerNo)
        {
            if (logId != 0)
            {
                this.SMSService.UpdateSMSLog(logId, chargeStatus, content, smsDirection, isSent.GetValueOrDefault(), targetPhoneNo, orderNo,outOrderNo,partenerNo);

                return Content("success");
            }
            return Content("error");
        }

        ///<summary>
        /// 
        /// </summary>
        /// <param name="imsis"></param>
        /// <returns>Mobile number</returns>
        public ActionResult GetMobiles(IEnumerable<string> imsi)
        {
            var actionResult = BuildResult<IMSIInfoViewModel>(() => true, () => SMSService.GetMobiles(imsi));

            return Content(actionResult.ToString());
        }

        #region 畅天游 CTU 企信通

        /// <summary>
        /// 企信通上行短信接口(CTU 畅天游)
        /// 发送到：106900608888, 1069033301128
        /// 短信内容：CS01+短信内容
        /// 
        /// -> Bind user's imsi and mobile number.
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="msg"></param>
        /// <param name="msg_id">消息唯一ID</param>
        /// <returns>Bind user imsi and mobile.</returns>
        public ActionResult Receive(string mobile, string msg, string msg_id)
        {
            var model = BuildResult<CTUMessageResult>(() => SMSService.CTUSMSReceive(mobile, msg, msg_id),
                () => new CTUMessageResult {  Result = CTUMessageResultCodes.SystemError});

            return Content(model.ToViewModelString());
        }

        /// <summary>
        /// 畅天游直充接口
        /// 可以通过QueryChannel查询到CTU动态接口
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="userNo"></param>
        /// <param name="phoneNo"></param>
        /// <returns></returns>
        //public ActionResult CTUPay(float amount, string userNo, string phoneNo)
        //{
        //    var actionResult = BuildResult<ChannelResult>(() => true, () => SMSService.CTUSMSPayRequest(amount, userNo, phoneNo), false);

        //    return Content(actionResult.ToString());
        //}

        /// <summary>
        /// Callback interface for CTU（畅天游）
        /// 
        /// Status: Success.
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="mobile"></param>
        /// <param name="amount"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public ActionResult Notify(string orderid, string mobile, string amount, string key)
        {
            // check key here
            var model = BuildResult<CTUChargeResult>(() => SMSService.CTUSMSPayCallback(orderid, mobile, amount, key),
                () => new CTUChargeResult { Result =  CTUChargeResultCodes.InternalException});

            return Content(model.ToViewModelString());
        }

        #endregion
    }
}
