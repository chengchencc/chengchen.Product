using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Framework.Core
{
    public static class Int32Extensions
    {
        static Random r = new Random();
        public static int Random(this List<int> source, List<int> probabilities)
        {
            if (source.Count != probabilities.Count || source == null || source.Count == 0)
            {
                throw new Exception("Random probabilities should be set.");
            }

            Dictionary<int, int> probTbl = new Dictionary<int, int>();
            for (int i = 0; i < probabilities.Count; i++)
            {
                probTbl[probabilities[i]] = source[i];
            }


            int max = probabilities.Sum();
            int selected = r.Next(1, max + 1);

            probabilities.Sort(delegate(int a, int b)
            {
                return b - a;
            });

            int probBase = 0;
            for (int i = 0; i < probabilities.Count; i++)
            {
                if (selected <= probabilities[i] + probBase)
                {
                    return probTbl[probabilities[i]];
                }
                probBase += probabilities[i];
            }

            return probTbl[probabilities[0]];

        }

        public static int CheckRange(this int value, int? max, int min)
        {
            if (max.HasValue)
            {
                return value >= min ? (value > max.GetValueOrDefault() ? max.GetValueOrDefault() : value) : min;
            }
            else
            {
                return value >= min ? value : min;
            }
        }

        public static T ToEnum<T>(this int value, T defaultValue) 
        {
            var type = typeof(T);
            if (type.IsEnum)
            {
                if (Enum.IsDefined(type, value))
                    return (T)Enum.Parse(type, value.ToString());
            }
            return defaultValue;
        }


        public static Boolean ToBoolean(this int trueOrFalse, int comparedValue = 1)
        {
            return trueOrFalse == comparedValue;
        }

        /// <summary>
        /// if the file size is 500 MB
        /// =>
        /// 500 MB
        /// 500000 KB
        /// 500000000 KB
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string ToFileSize(this int length)
        {
            string size = string.Empty;
            float num = length;

            if (length < 0)
            {
                return size;
            }

            if (length < 1024)
            {
                size = string.Format("{0:N0} B", length);
            }
            else if (length > 1024)
            {
                if (length < 1048576)
                {
                    size = string.Format("{0:N2} KB", num / 1024);
                }
                else
                {
                    size = string.Format("{0:N2} MB", num / 1048576);
                }
            }

            return size;
        }

    }
}
