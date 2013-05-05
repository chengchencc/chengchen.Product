using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;

namespace BlackMamba.Framework.Core
{
    public interface IRequestRepository
    {
        NameValueCollection Header { get; }

        NameValueCollection QueryString { get; }

        IDictionary HttpContextItems { get; }

        string UserHostName { get; }

        string RawUrl { get; }

        string ClientIP { get; }

        string SessionId { get; }

        string UserAgent { get; }

        /// <summary>
        /// Request.Url.query
        /// </summary>
        string QueryUrl { get; }

        Byte[] GetRequestPostStream();

        string PostedDataString { get; }

        string GetValueFromHeadOrQueryString(string key);
    }
}
