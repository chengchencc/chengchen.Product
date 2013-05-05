using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using BlackMamba.Framework.Core;

namespace BlackMamba.Framework.Core
{
    public static class StringExtensions
    {
        #region Consts
        const string TRUE = "true";
        const string FALSE = "false";
        public const string MatchEmailPattern =
           @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
    + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
				[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
    + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
				[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
    + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

        const string CHINESE_PATTERN = @"[\u4e00-\u9fa5\uFF00-\uFFFF\s《》]*";

        const string CHINESE_SYMBOL = @"[\u0391-\uFFE5]";
        const string CHINESE_CHARACTER = @"[\\u4e00-\\u9fa5]";
        const string CHINESE_SYMBOL_AND_CHARACTER = @"[\u0391-\uFFE5]";

        private static Dictionary<string, string> CachedEncodedStringValue = new Dictionary<string, string>();
        #endregion

        #region Convertion
        public static Int32 ToInt32(this bool booleanValue)
        {
            return Convert.ToInt32(booleanValue);
        }

        public static Int32 ToInt32(this string integerStr)
        {
            return integerStr.ToInt32(0);
        }
        
        public static Int32 ToInt32(this string integerStr, Func<Int32> getDefaultValue)
        {
            return integerStr.ToInt32(getDefaultValue());
        }

        public static Int32 ToInt32(this string integerStr, Int32 defaultValue)
        {
            var value = 0;
            var canParse = int.TryParse(integerStr, out value);
            if (!canParse) value = defaultValue;

            return value;
        }

        public static Int64 ToInt64(this string value)
        {
            if (value.IsNullOrEmpty())
            {
                return 0;
            }

            var ret = default(long);
            long.TryParse(value, out ret);

            return ret;
        }

        public static double ToDouble(this string doubleStr)
        {
            double val = 0.0;
            double.TryParse(doubleStr, out val);
            return val;
        }

        public static float ToFloat(this string floatStr)
        {
            float val = 0.0f;
            float.TryParse(floatStr, out val);
            return val;
        }

        public static Boolean ToBoolean(this string trueOrFalse)
        {
            var ret = false;
            if (!string.IsNullOrEmpty(trueOrFalse))
            {
                ret = trueOrFalse.Trim().EqualsOrdinalIgnoreCase(TRUE);
            }

            return ret;
        }

        public static DateTime ToExactDateTime(this string dateTimeString, string format)
        {
            var ret = DateTime.MinValue;

            if (!dateTimeString.IsNullOrEmpty())
            {
                DateTime.TryParseExact(dateTimeString.Trim(), format, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out ret);
            }
            return ret;
        }
        #endregion

        #region Encoding

        /// <summary>
        /// This function will encode the special char with url encode strategy
        /// </summary>
        /// <param name="value"></param>
        /// <param name="special"></param>
        /// <returns></returns>
        public static string EncodeSpecial(this string value, string special)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var encoded = string.Empty;

                if (CachedEncodedStringValue.ContainsKey(special)) encoded = CachedEncodedStringValue[special];
                else
                {
                    encoded = HttpUtility.UrlEncode(special);
                    CachedEncodedStringValue[special] = encoded;
                }

                return value.Replace(special, encoded);
            }

            return value;
        }

        /// <summary>
        /// Will call HttpUtility.UrlEncode method
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string UrlEncode(this string value)
        {
            return HttpUtility.UrlEncode(value);
        }

        /// <summary>
        /// 对URL中的中文进行编码
        /// 其中也包括空格
        /// 全角符号及《》
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EncodeChineseChars(this string value)
        {
            // chinese characters & space chars


            var evaluator = new MatchEvaluator(match =>
            {
                var ret = match.Value.UrlEncode();
                return ret;
            });

            value = Regex.Replace(value, CHINESE_PATTERN, evaluator);

            return value;
        }

        /// <summary>
        /// Will call HttpUtility.UrlEncode method
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string UrlEncode(this string value, Encoding encoding)
        {
            return HttpUtility.UrlEncode(value, encoding);
        }
        #endregion

        #region String Util

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Will not throw exception if the string is null
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Lower(this string value)
        {
            if (value.IsNullOrEmpty())
            {
                return string.Empty;
            }
            else
            {
                return value.ToLower();
            }
        }

        /// <summary>
        /// Will not throw exception if the string is null
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Upper(this string value)
        {
            if (value.IsNullOrEmpty())
            {
                return string.Empty;
            }
            else
            {
                return value.ToUpper();
            }
        }

        /// <summary>
        /// equals to string.Equals(A, B);
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool EqualsOrdinalIgnoreCase(this string A, string B)
        {
            return string.Equals(A, B, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Make sure the string is not null
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string MakeSureNotNull(this string val)
        {
            if (val == null)
            {
                return string.Empty;
            }
            else
            {
                return val;
            }
        }


        public static bool IsEmail(this string emailStr)
        {
            return Regex.IsMatch(emailStr, MatchEmailPattern) && !Regex.IsMatch(emailStr, @"[\u4e00-\u9fa5\uFF00-\uFFFF]");
        }

        public static string ConfigValue(this string key)
        {
            return SingletonBase<ConfigurableSet>.Instance[key];
        }

        public static string TakeLength(this string value, int wantLength)
        {
            if (!value.IsNullOrEmpty())
            {
                if (value.Length >= wantLength) value = value.Substring(0, wantLength);
            }

            return value;
        }

        /// <summary>
        /// UTF-8
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this string str)
        {
            if (str == null) return null;

            return Encoding.UTF8.GetBytes(str);
        }
        #endregion


    }
}