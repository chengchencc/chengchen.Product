using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using BlackMamba.Framework.Core;

namespace BlackMamba.Billing.Domain.Helpers
{
    public class WebHelper
    {
        public static void SetResponseForCMWAP()
        {
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.GetEncoding(EncodingNames.GBK);
            HttpContext.Current.Response.ContentType = "application/octet-stream";
        }

        public static string GetPostData(Stream stream)
        {
            var postData = string.Empty;
            try
            {
                StreamReader sr = new StreamReader(stream, Encoding.GetEncoding("gb2312"));
                postData = sr.ReadToEnd();
                sr.Close();
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("GetPostData" + ex.Message);
            }
            return postData;
        }


    }
}
