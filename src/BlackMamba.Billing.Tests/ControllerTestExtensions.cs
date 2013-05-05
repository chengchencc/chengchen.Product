using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace BlackMamba.Billing.Tests
{
    public static class ControllerTestExtensions
    {
        public static bool IsContentActionResult(this ActionResult result)
        {
            return result is ContentResult;
        }

        public static string ContentResultData(this ActionResult result)
        {
            var contentAction = result as ContentResult;
            if (contentAction != null)
            {
                return contentAction.Content;
            }

            return string.Empty;
        }

        public static bool IsRedirectResult(this ActionResult result)
        {
            return result is RedirectResult;
        }

        public static string RedirectedUrl(this ActionResult result)
        {
            var redirectResult = result as RedirectResult;
            if (redirectResult != null)
            {
                return redirectResult.Url;
            }

            return string.Empty;
        }

    }
}
