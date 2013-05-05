using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;
using BlackMamba.Framework.Core;
using BlackMamba.Billing.Domain;

namespace BlackMamba.Billing.Domain
{
    public class RESTfulClient : IRESTfulClient
    {
        /// <summary>
        /// For response, only support gbk encoding..
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeoutMilliSeconds"></param>
        /// <returns></returns>
        public string Get(string url, int timeoutMilliSeconds = 0)
        {
            var content = string.Empty;
            try
            {
                var client = new RestClient(url);
                if (timeoutMilliSeconds > 0) client.Timeout = timeoutMilliSeconds;

                var request = new RestRequest();
                request.Method = Method.GET;

                var response = client.Execute(request);
                if (response.RawBytes != null) content = Encoding.GetEncoding(EncodingNames.GBK).GetString(response.RawBytes);
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(ex.Message);
                LogHelper.WriteError(ex.StackTrace);
            }

            return content;
        }
        /// <summary>
        /// For response, only support gbk encoding..
        /// </summary>
        /// <param name="url"></param>
        /// <param name="paras"></param>
        /// <param name="timeoutMilliSconds"></param>
        /// <returns></returns>
        public string Post(string url, Dictionary<string, string> paras, int timeoutMilliSconds = 0)
        {

            var content = string.Empty;

            try
            {
                var client = new RestClient(url);
                if (timeoutMilliSconds > 0) client.Timeout = timeoutMilliSconds;

                var request = new RestRequest(Method.POST);
                foreach (var param in paras)
                {
                    request.AddParameter(param.Key, param.Value);
                }

                var response = client.Execute(request);
                content = Encoding.GetEncoding(EncodingNames.GBK).GetString(response.RawBytes);
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(ex.Message);
                LogHelper.WriteError(ex.StackTrace);
            }

            return content;

        }


    }
}
