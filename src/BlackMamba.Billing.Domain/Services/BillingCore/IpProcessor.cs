using System;
using System.Collections.Generic;
using com.yeepay.cmbn;
using BlackMamba.Framework.Core;
using BlackMamba.Framework.RedisMapper;
using SubSonic.Oracle.Repository;
using BlackMamba.Billing.Domain.Common;
using BlackMamba.Billing.Domain.Mappers;
using BlackMamba.Billing.Models.Billing;
using BlackMamba.Billing.Domain.Services;
using BlackMamba.Billing.Domain.Services.BillingCore;
using StructureMap;
using NLog;

namespace BlackMamba.Billing.Domain
{
    public class IpProcessor
    {
        #region Prop

        public IRESTfulClient RESTFulService
        {
            get
            {
                if (_restfulService == null)
                {
                    _restfulService = ObjectFactory.GetInstance<IRESTfulClient>();
                }
                return _restfulService;
            }
            set
            {
                _restfulService = value;
            }
        }private IRESTfulClient _restfulService;
        public IEMailService EMailService
        {
            get
            {
                if (_emailService == null)
                {
                    _emailService = ObjectFactory.GetInstance<IEMailService>();
                }
                return _emailService;
            }
            set
            {
                _emailService = value;
            }
        }private IEMailService _emailService;
        #endregion

        const string IP138 = "http://iframe.ip138.com/ic.asp";

        public void Check()
        {
            var response = RESTFulService.Get(IP138, 5000);
            string regPattern = @"\[(\S*)]";
            var match = RegexHelper.GetMatch(response, regPattern);
            if (match.Success)
            {
                var ip = match.Groups[1].Value;
                if (!string.IsNullOrEmpty(ip))
                {
                    if (string.IsNullOrEmpty(IPCache.IP) || ip != IPCache.IP)
                    {
                        this.SendEmail("New IP:" + ip);
                        IPCache.IP = ip;
                    }
                }
            }
        }

        public void SendEmail(string body)
        {
            var smtpServer = "smtp.qq.com";
            var fromEmail = "214636584@qq.com";
            var fromPassword = "cc880216";
            var toMyQQEmail = "123439887@qq.com";
            var toGnQQEmail = "278412113@qq.com";
            var subject = "NewIP";
            var content = body;
            try
            {
                EMailService.SendMail(smtpServer, fromEmail, fromPassword, toMyQQEmail, subject, content);
                LogManager.GetLogger("ErrorLogger").Info(string.Format("smsServer:{0}\r\n From{1}\r\n FromPasword:{2}\r\n To:{3}\r\n Subject:{4}\r\n Content:{5}", smtpServer, fromEmail, fromPassword, toMyQQEmail, subject, content));
                //EMailService.SendMail(smtpServer, fromEmail, fromPassword, toGnQQEmail, subject, content);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ErrorLogger").Error(string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
            }

        }

    }

    public static class IPCache
    {
        public static string IP
        {
            get
            {
                return ip;
            }
            set
            {
                ip = value;
            }
        }private static string ip = string.Empty;
    }
}
