using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using WebMatrix.WebData;

namespace BlackMamba.Billing.Website.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            var cookie = Request.Cookies["USERLOGIN"];

            if (cookie != null)
            {
                var validUserName = ConfigurationManager.AppSettings["UserName"];
                var validPassword = ConfigurationManager.AppSettings["Password"];
                var encryptedPassword = FormsAuthentication.HashPasswordForStoringInConfigFile(validPassword, "MD5");

                if (cookie["USERNAME"] == validUserName && cookie["PASSWORD"] == encryptedPassword)
                {
                    return RedirectToAction("OrderManage", "PayCenterUI");
                }
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(string UserName, string Password)
        {
            var validUserName = ConfigurationManager.AppSettings["UserName"];
            var validPassword = ConfigurationManager.AppSettings["Password"];
            if (UserName == validUserName && Password == validPassword)
            {
                var cookie = new HttpCookie("USERLOGIN");
                cookie.Expires = DateTime.Now.AddDays(7);

                var encryptedPassword = FormsAuthentication.HashPasswordForStoringInConfigFile(Password, "MD5");
                cookie["USERNAME"] = UserName;
                cookie["PASSWORD"] = encryptedPassword;
                Response.Cookies.Add(cookie);

                return RedirectToAction("OrderManage", "PayCenterUI");
            }
            else
            {
                return View();
            }
        }

        //
        // POST: /Account/LogOff
        public ActionResult LogOff()
        {
            var cookie = Request.Cookies["USERLOGIN"];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }
          
            return Redirect("/Account/Login");
        }

    }
}
