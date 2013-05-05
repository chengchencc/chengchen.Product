using System;
using System.Text.RegularExpressions;

namespace BlackMamba.Framework.Core
{
    public class RegexHelper
    {
        public static MatchCollection GetMatchCollection(string content, string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var matches = regex.Matches(content);

            return matches;
        }

        public static Match GetMatch(string content, string pattern, RegexOptions options)
        {
            var rx = new Regex(pattern, options);

            var match = rx.Match(content);

            return match;
        }

        public static Match GetMatch(string content, string pattern)
        {
            return GetMatch(content, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        public static string GetMatchedGroupValue(string content, string pattern, string groupName, Action<string> successAction = null)
        {
            var match = GetMatch(content, pattern);
            var ret = string.Empty;

            if (match.Success)
            {
                var gp = match.Groups[groupName];
                if (gp != null && gp.Success)
                {
                    ret = gp.Value;

                    if (successAction != null)
                    {
                        successAction(ret);
                    }
                }
            }

            return ret;
        }

        public static bool IsMatch(string content, string pattern)
        {
            return IsMatch(content, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        }

        public static bool IsMatch(string content, string pattern, RegexOptions options)
        {
            var rx = new Regex(pattern, options);

            return rx.IsMatch(content);
        }
    }
}
