using System.Text.RegularExpressions;
using BlackMamba.Framework.Core;
using System;
using System.Text;
using BlackMamba.Billing.Domain.Helpers;
using System.Security.Cryptography;
using RestSharp.Contrib;
using BlackMamba.Billing.Domain.ViewModels.Billing;

namespace BlackMamba.Billing.Domain
{
    public static class StringExtensions
    {
        public static string MakeSureUnicodeStringByteLength(this string value, int limitLength)
        {
            // there are 2 bytes for one character for unicode encoding, C# uses Unicode which is 2 bytes per character by default
            if (value.Length > limitLength / 2)
            {
                value = value.Substring(0, limitLength / 2);
            }

            return value;
        }

        public static DateTime ToCNDate(this string dateStr)
        {
            IFormatProvider culture = new System.Globalization.CultureInfo("zh-CN", true);
            DateTime dateTime = DateTime.MinValue;
            DateTime.TryParse(dateStr, culture, System.Globalization.DateTimeStyles.AssumeLocal, out dateTime);
            return dateTime;
        }

        /// <summary>
        /// Reformat mobile
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public static string MobileFormat(this string mobile)
        {
            if (mobile.IsNullOrEmpty()) return string.Empty;

            if (mobile.StartsWith("9")) return mobile;

            return mobile.Substring(mobile.IndexOf("1"));
        }

        const string VALUE_CONTAINS_CHINESE = @"=(?<v>.*?(?>[\u4e00-\u9fa5\uFF00-\uFFFF\s《》]+)[^&]*)";
        public static string RemoveAtChar(this string url)
        {
            if (!string.IsNullOrEmpty(url))
                return url.TrimStart(ASCII.AT_CHAR);

            return url;
        }

        /// <summary>
        /// Please notice that the input param is key instead of the key value
        /// e.g.
        /// if there is one method, say, method(string keyword)
        /// and if you want to decode the keyword value
        /// you should write "keyword".UrlDecodeFromRawQuery();
        /// The program will decode it with gbk or utf-8;
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string UrlDecodeFromRawQuery(this string key, IRequestRepository requestRepo, string encode = "")
        {
            var rawUrl = requestRepo.RawUrl;
            if (!rawUrl.IsNullOrEmpty())
            {
                var match = RegexHelper.GetMatch(rawUrl, key + VALUE_CONTAINS_CHINESE);

                if (match.Success)
                {
                    return match.Groups["v"].Value;
                }
            }

            return key.UrlDecodeFromRawQuery(requestRepo.QueryUrl, encode);
        }

        public static string UrlDecodeFromRawQuery(this string key, string rawUrl, string encode)
        {
            if (!rawUrl.IsNullOrEmpty())
            {
                var rawValue = HttpHelper.GetUrlParam(key, rawUrl);

                if (encode.Equals(EncodingNames.GBK))
                {
                    return HttpHelper.GetParamDecode_GBK(rawValue);
                }
                else if (encode.Equals(EncodingNames.UTF8))
                {
                    return HttpHelper.GetParamDecode_UTF8(rawValue);
                }
                else
                {
                    return HttpHelper.GetParamDecode(rawValue);
                }
            }

            return string.Empty;
        }

        public static string SHA1Hash(this string str)
        {
            return Convert.ToBase64String(new SHA1CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(str.EncodeChineseChars())));
        }

    }
}