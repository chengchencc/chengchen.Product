using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BlackMamba.Billing.Domain.Services;
using NLog;

namespace BlackMamba.Billing.Website.Controllers
{
    public class HomeController : MvcControllerBase
    {
        protected internal override bool IsWriteActionLog { get { return false; } }

        public IEMailService MailService { get; set; }
        public HomeController(IEMailService mailService)
        {
            MailService = mailService;
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            var smtpServer = "smtp.qq.com";
            var fromEmail = "214636584@qq.com";
            var fromPassword = "cc880216";
            var toMyQQEmail = "123439887@qq.com";
            var toGnQQEmail = "278412113@qq.com";
            var subject = "Test";
            var content = "test content!";
            try
            {
                MailService.SendMail(smtpServer, fromEmail, fromPassword, toMyQQEmail, subject, content);
                LogManager.GetLogger("ErrorLogger").Info(string.Format("smsServer:{0}\r\n From{1}\r\n FromPasword:{2}\r\n To:{3}\r\n Subject:{4}\r\n Content:{5}", smtpServer, fromEmail, fromPassword, toMyQQEmail, subject, content));
                MailService.SendMail(smtpServer, fromEmail, fromPassword, toGnQQEmail, subject, content);                
                return Content("success");
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ErrorLogger").Error(string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
                return Content("fail");
            }
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
