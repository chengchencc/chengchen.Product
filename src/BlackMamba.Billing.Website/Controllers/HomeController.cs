using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BlackMamba.Billing.Domain.Services;

namespace BlackMamba.Billing.Website.Controllers
{
    public class HomeController : MvcControllerBase
    {
        protected internal override bool IsWriteActionLog { get { return false; } }

        public IPaymentsService PaymentsService { get; set; }
        public HomeController(IPaymentsService payments)
        {
            PaymentsService = payments;
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
