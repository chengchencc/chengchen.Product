using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace BlackMamba.Framework.Core
{
    public class HttpHelper
    {
        const string VALUE_CONTAINS_CHINESE = @"[(\u4e00-\u9fa5)(\uFF00-\uFFFF)《》\+]+";

        public static string GetUrlParamEncodeUTF8(string key)
        {
            var text = GetUrlParamDecode(key);
            return HttpUtility.UrlEncode(text, Encoding.UTF8);
        }

        /// <summary>
        /// The key is the variable name
        /// In this case, we will use the raw value of the querystring
        /// otherwise, you can use the method: GetParamDecode(string input)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetUrlParamDecode(string key)
        {
            string input = GetUrlParam(key).ToLower();

            return GetParamDecode(input);
        }

        public static string GetParamDecode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var result = string.Empty;

            //首先用utf-8进行解码
            var utf8Decoded = HttpUtility.UrlDecode(input, Encoding.UTF8);
            // 将已经解码的字符再次进行编码.            
            var utf8Encoded = EncodeChineseCharacterOnly(utf8Decoded, Encoding.UTF8).ToLower();
            //var utf8Encoded = HttpUtility.UrlEncode(utf8Decoded, Encoding.UTF8).ToLower();

            var gbk = Encoding.GetEncoding("gb2312");
            var gbkDecoded = HttpUtility.UrlDecode(input, gbk);

            // 编码一致，但还有可能是GBK
            if (string.Compare(input, utf8Encoded, StringComparison.OrdinalIgnoreCase) == 0)
            {
                // 判断两个解码后的值，若相同取UTF8，若不同，取GBK
                //var gbkEncoded = HttpUtility.UrlEncode(gbkDecoded, gbk);
                var gbkEncoded = EncodeChineseCharacterOnly(gbkDecoded, gbk);
                // 如果还相同，取长度短的
                result = gbkDecoded.Length <= utf8Decoded.Length ? gbkDecoded : utf8Decoded;

            }
            else
            {
                //与原来编码进行对比，如果不一致说明解码未正确，用gb2312进行解码
                result = gbkDecoded;
            }

            return result;
        }

        public static string EncodeChineseCharacterOnly(string input, Encoding encoding)
        {
            var matches = RegexHelper.GetMatchCollection(input, VALUE_CONTAINS_CHINESE);
            if (matches != null && matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    var value = match.Value;

                    var index = input.IndexOf(value);
                    if (!string.IsNullOrEmpty(value) && index >= 0)
                    {
                        var prefix = input.Substring(0, index);
                        var suffix = string.Empty;
                        if ((index + value.Length) < input.Length) suffix = input.Substring(index + value.Length);

                        input = string.Format("{0}{1}{2}", prefix, HttpUtility.UrlEncode(value, encoding), suffix);
                    }
                }
            }

            return input;
        }

        public static string GetUrlParam(string key)
        {
            if (System.Web.HttpContext.Current != null)
            {
                string query = HttpContext.Current.Request.Url.Query;

                return GetUrlParam(key, query);
            }
            return string.Empty;
        }

        public static string GetUrlParam(string key, string rawUrl)
        {
            string query = rawUrl;

            if (query != null && query.Length > 0)
            {
                int index = 0;
                index = query.IndexOf(key + "=");
                if (index >= 0)
                {
                    query = query.Substring(key.Length + 1 + index);
                    index = query.IndexOf('&');
                    if (index >= 0)
                        query = query.Substring(0, index);
                    return query;
                }
            }
            return string.Empty;
        }

        public static string GetPostData()
        {
            Stream stream = HttpContext.Current.Request.InputStream;
            var postData = string.Empty;
            try
            {
                StreamReader sr = new StreamReader(stream, Encoding.GetEncoding(EncodingNames.GB2312));
                postData = sr.ReadToEnd();
                sr.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //LogHelper.Error("GetPostData" + ex.Message);
            }
            return postData;
        }

        public static string Get(string url)
        {
            String resultStr = string.Empty;
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.Method = "GET";
                using (WebResponse wr = req.GetResponse())
                {
                    StreamReader sr = new StreamReader(wr.GetResponseStream(), System.Text.Encoding.Default);
                    resultStr = sr.ReadToEnd();
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //LogHelper.Error(ex.Message);
            }

            return resultStr;
        }


        public static string GetParamDecode_UTF8(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var result = string.Empty;

            //首先用utf-8进行解码
            var utf8Decoded = HttpUtility.UrlDecode(input, Encoding.UTF8);
            // 将已经解码的字符再次进行编码.            
            //var utf8Encoded = EncodeChineseCharacterOnly(utf8Decoded, Encoding.UTF8).ToLower();

            return utf8Decoded;
        }


        public static string GetParamDecode_GBK(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var gbk = Encoding.GetEncoding(EncodingNames.GB2312);
            var gbkDecoded = HttpUtility.UrlDecode(input, gbk);

            return gbkDecoded;
        }
    }
}
