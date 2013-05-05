using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace BlackMamba.Billing.Website.Controllers.Base
{
    public class UIBaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var cookie = Request.Cookies["USERLOGIN"];
            bool hasLogin = false;
            if (cookie != null)
            {
                var validUserName = ConfigurationManager.AppSettings["UserName"];
                var validPassword = ConfigurationManager.AppSettings["Password"];
                var encryptedPassword = FormsAuthentication.HashPasswordForStoringInConfigFile(validPassword, "MD5");
                if (cookie["USERNAME"] == validUserName && cookie["PASSWORD"] == encryptedPassword)
                {
                    hasLogin = true;
                }
            }

            if (!hasLogin)
            {
                filterContext.Result = new RedirectToRouteResult
                  (
                      new RouteValueDictionary
                      (
                          new
                          {
                              controller = "Account",
                              action = "Login"
                          }
                      )
                  );
                return;
            }
        }
    }
}
