using System.Collections.Specialized;
using System.Web;
using BlackMamba.Framework.Core;
using System.Collections;

using BlackMamba.Billing.Domain.Helpers;

namespace BlackMamba.Billing.Domain
{
    public class RequestRepository : IRequestRepository
    {
        public const string SERVER_VARIABLES_HOST = "HTTP_HOST";

        // for ngix
        public const string SERVER_VARIABLES_X_FORWARDED_FOR = "X-FORWARDED-FOR";
        public const string SERVER_VARIABLES_X_REAL_IP = "X-REAL-IP";

        // for no ngix
        public const string SERVER_VARIABLES_FORWARDED_FOR = "HTTP_X_FORWARDED_FOR";
        public const string SERVER_VARIABLES_ADDR = "REMOTE_ADDR";


        public NameValueCollection Header
        {
            get
            {
                return HttpContext.Current.Request.Headers;
            }
        }

        public NameValueCollection QueryString
        {
            get
            {
                return HttpContext.Current.Request.QueryString;
            }
        }

        public IDictionary HttpContextItems
        {
            get { return HttpContext.Current.Items; }
        }

        public string RawUrl
        {
            get
            {
                var rawUrl = HttpContext.Current.Request.RawUrl;

                return rawUrl;
            }
        }

        public string PostedDataString
        {
            get
            {
                var post = string.Empty;
                if (HttpContext.Current.Items[HeaderKeys.POST] == null)
                {
                    post = WebHelper.GetPostData(HttpContext.Current.Request.InputStream);
                    HttpContext.Current.Items[HeaderKeys.POST] = post;
                }
                else
                {
                    post = HttpContext.Current.Items[HeaderKeys.POST].ToString();
                }
                return post;
            }
        }


        public string GetValueFromHeadOrQueryString(string key)
        {
            if (key.IsNullOrEmpty()) return null;

            var headerValue = default(string);
            var queryStringValue = default(string);

            if (Header != null)
            {
                headerValue = Header[key];
            }

            if (headerValue.IsNullOrEmpty() && QueryString != null)
            {
                queryStringValue = QueryString[key];
                if (!queryStringValue.IsNullOrEmpty())
                    headerValue = queryStringValue;
            }

            return headerValue;
        }

        public string UserHostName
        {
            get { return HttpContext.Current.Request.ServerVariables[SERVER_VARIABLES_HOST]; }
        }

        public string QueryUrl
        {
            get { return HttpContext.Current.Request.Url.Query; }
        }

        public string ClientIP
        {
            get
            {
                var serverVariables = HttpContext.Current.Request.ServerVariables;

                // get the ip from ngix forwarded for
                var ip = serverVariables[SERVER_VARIABLES_X_FORWARDED_FOR];
                // get the ip from ngix real ip
                if (ip.IsNullOrEmpty()) { ip = serverVariables[SERVER_VARIABLES_X_REAL_IP]; }

                // get the ip from forwarded for if no ngix
                if (ip.IsNullOrEmpty()) { ip = serverVariables[SERVER_VARIABLES_FORWARDED_FOR]; }
                if (ip.IsNullOrEmpty()) { ip = serverVariables[SERVER_VARIABLES_ADDR]; }

                return ip.TakeLength(32);
            }
        }

        public string SessionId
        {
            get
            {
                if (HttpContext.Current.Session != null)
                    return HttpContext.Current.Session.SessionID;
                return string.Empty;
            }
        }

        public string UserAgent
        {
            get { return HttpContext.Current.Request.UserAgent; }
        }



        public byte[] GetRequestPostStream()
        {
            return null;
        }
    }
}
