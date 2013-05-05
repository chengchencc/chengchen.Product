using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AlipayFunction = Alipay.Class.Function;
using System.Xml;
using BlackMamba.Framework.Core;
using NLog;
using Alipay.Class;
using System.Text;
using BlackMamba.Billing.Domain.Services;
using BlackMamba.Billing.Domain.Common;

namespace BlackMamba.Billing.Website.Controllers
{
    
    public class AlipayController : MvcControllerBase
    {
        public IPaymentsService PaymentsService { get; set; }
        public AlipayController(IPaymentsService paymentsService)
        {
            PaymentsService = paymentsService;
        }
      
        [HttpPost] //支付宝将以POST方式调用该地址
        [ValidateInput(false)] //支付宝post的notify_data的数据为xml格式
        public ActionResult Notify(string notify_data, string sign, string cbUrl)
        {
            if (!VerifySignature(notify_data, sign))
            {
                Logger.Info(">>>>>>>> sign error <<<<<<<<");
                return Content("fail");
            }

            //获取交易状态
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(notify_data);
            }
            catch
            {
                Logger.Info(">>>>>>>> xml parse error <<<<<<<<");
                return Content("fail");
            }

            string trade_status = AlipayFunction.GetStrForXmlDoc(xmlDoc, "notify/trade_status");
            if (!trade_status.Equals(BillingConsts.ALIPAY_TRADE_FINISHED))
            {
                return Content("fail");
            }
            else
            {
                PaymentsService.AlipaySDKCallback(xmlDoc, cbUrl);

                //成功必须在页面上输出success，支付宝才不会再发送通知
                return Content("success");
            }
        }

        public ActionResult CreateOrder(float amount,string productName="",string productDesc="",string userId = "")
        {
            var orderNo = PaymentsService.CreateOrder(productName, amount, productDesc,userId);
            return Content(orderNo);
        }


        public ActionResult TestWapPay()
        {
            return View();
        }

        public ActionResult WapPay(string productName, string fee, string userId, string callBackUrl)
        {
            Func<bool> checkParameter = () => !string.IsNullOrEmpty(productName) && fee.ToFloat() > 0.0f
                                            && !string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(callBackUrl);

            Func<string> getRedirectUrl = () => PaymentsService.InitAlipayWapPay(productName, fee, userId, callBackUrl);

            return BuildRedirectResult(checkParameter, getRedirectUrl, true);
        }
        
        /// 该通知主要功能是：对于返回页面（call_back_url.aspx）做补单处理。如果没有收到该页面返回的 success 信息，支付宝会在24小时内按一定的时间策略重发通知
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult WapNotify(string notify_data, string sign, string service, string v, string sec_id)
        {
            notify_data = PaymentsService.VerifyAlipayWapNotify(notify_data, sign, service, v, sec_id);
            if (string.IsNullOrEmpty(notify_data)) 
            {
                return Content("fail");
            }

            //获取交易状态
            string trade_status = Function.GetStrForXmlDoc(notify_data, "notify/trade_status");

            if (!trade_status.Equals("TRADE_FINISHED"))
            {
                return Content("fail");
            }
            else
            {
                // Process order here   
                PaymentsService.AlipayWapNotify(notify_data);

                return Content("success");
            }
        }

        public ActionResult WapCallback(string result, string sign)
        {
            SortedDictionary<string, string> requestParams = GetRequestGet();

            LoggingWapCallbackParameters(result, sign, requestParams);

            bool isVerify = Function.Verify(requestParams, sign, Config.Alipaypublick, Config.Input_charset_UTF8);
            if (!isVerify)
            {
                //验签出错，可能被别人篡改数据
                return Content("fail");
            }

            if (result.EqualsOrdinalIgnoreCase("success"))
            {
                // Process order 
                PaymentsService.AlipayWapCallback(result, sign, requestParams);

                return Content("success");
            }
            else
            {
                return Content("fail");
            }
        }

        public ActionResult WapCancel()
        {
            // cancel go to call back page, just go to index page by default
            return RedirectToAction("Index", "AppStoresWapUI");
        }

        /// <summary>
        /// 获取所有Callback参数
        /// </summary>
        /// <returns>SortedDictionary格式参数</returns>
        private SortedDictionary<string, string> GetRequestGet()
        {
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
            string query = Request.Url.Query.Replace("?", "");
            if (!string.IsNullOrEmpty(query))
            {
                string[] coll = query.Split('&');

                string[] temp = { };

                for (int i = 0; i < coll.Length; i++)
                {
                    temp = coll[i].Split('=');

                    sArray.Add(temp[0], temp[1]);
                }
            }
            return sArray;
        }

        private static void LoggingWapCallbackParameters(string result, string sign, SortedDictionary<string, string> sArrary)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var p in sArrary)
            {
                sb.AppendFormat(" {0}={1}", p.Key, p.Value);
            }

            LogManager.GetLogger("PaymentsTest").Info(string.Format(">>>>> WAP CALLBACK: result:{0} sign:{1}  {2}", result, sign, sb.ToString()));
        }

        private bool VerifySignature(string notify_data, string sign)
        {
            if (!ConfigKeys.NEED_VERIFY_SIG.ConfigValue().ToBoolean())
            {
                return true;
            }

            //获取notify_data数据，需要添加notify_data=
            //不需要解密，直接是明文xml格式
            string notifyData = "notify_data=" + notify_data;
            
            // self sign
            //sign = Alipay.Class.RSAFromPkcs8.sign(notify_data, ConfigKeys.MERCHANT_PRIVATE_KEY.ConfigValue(), "utf-8");
            
            //验证签名
            return AlipayFunction.Verify(notifyData, sign, ConfigKeys.ALI_PUBLIC_KEY.ConfigValue(), "utf-8");
        }
    }
}
