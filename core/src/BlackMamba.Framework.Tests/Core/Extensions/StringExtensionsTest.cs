using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using BlackMamba.Framework.Core;

namespace Framework.Core.Tests.NCore.Extensions
{
    public class StringExtensionsTest
    {
        [Fact]
        public void EmailFormatValidation_test()
        {
            Assert.False("djdj@fdfa@fdf.com".IsEmail());
            Assert.False("@goole.com".IsEmail());
            Assert.False("google@.com".IsEmail());
            Assert.True("djdj@fdfa.com".IsEmail());
            Assert.True("djdj@fdfa.ddd".IsEmail());
            Assert.True("d.j.dj@fdfa.ddd.com".IsEmail());
            Assert.False("djdj@fdfa".IsEmail());
        }

        [Fact]
        public void MakeSureNotNull_Test()
        {
            string a = null;
            Assert.Equal(string.Empty, a.MakeSureNotNull());

            Assert.Equal("a", "a".MakeSureNotNull());
        }

        [Fact]
        public void ToInt32_Tests()
        {
            Assert.Equal(1, "1".ToInt32());
            Assert.Equal(0, "".ToInt32());
            Assert.Equal(0, "0".ToInt32());
            Assert.Equal(0, "33a".ToInt32());
            Assert.Equal(0, "false".ToInt32());
            Assert.Equal(1, true.ToInt32());
            Assert.Equal(0, "1.0".ToInt32());
            Assert.Equal(111, "1.0".ToInt32(111));
            Assert.Equal(0, "1.111".ToInt32());
            Assert.Equal(222, "1.111".ToInt32(() => 222));

            var d = default(string);
            Assert.Equal(0, d.ToInt32());
        }

        [Fact]
        public void Compare_Test()
        {
            Assert.True("aa".EqualsOrdinalIgnoreCase("Aa"));
            Assert.True("Aa".EqualsOrdinalIgnoreCase("aa"));
            Assert.False("Aa ".EqualsOrdinalIgnoreCase("aa"));
        }

        [Fact]
        public void EncodeTest()
        {
            Assert.Equal("%7c", "|".EncodeSpecial("|"));
            Assert.Equal("%7c%25", "|%".EncodeSpecial("|%"));
            Assert.Equal("%7c%3b", "|;".EncodeSpecial("|;"));
            Assert.Equal("%7c%2c", "|,".EncodeSpecial("|,"));
            Assert.Equal("%7c%26", "|&".EncodeSpecial("|&"));
            Assert.Equal("%7c%26", "|&".EncodeSpecial("|&"));

            var nullstring = default(string);
            Assert.Equal(default(string), nullstring.EncodeSpecial("|"));
        }

        [Fact(Skip = "Benchmark")]
        public void Encode_bench_mark_with_string_replace()
        {
            var time = DateTime.Now;
            for (int i = 0; i < 10000; i++)
            {
                var input = "@!#$%^*%^%&(*())(&+JKHKJHKGYJGJKDDDLJSDKL<NC<MNX<M565766ewu934938435793478";
                var ret = input.EncodeSpecial("%").EncodeSpecial("|").EncodeSpecial("&").EncodeSpecial(",").EncodeSpecial(";");
            }

            var span = DateTime.Now - time;

            Console.WriteLine(span.Milliseconds);
        }

        [Fact]
        public void UrlEncode_for_chinese()
        {
            var url = "游戏";
            var encoded = url.UrlEncode();
            var encodedWithEncoding = url.UrlEncode(Encoding.GetEncoding(EncodingNames.UTF8));

            Assert.Equal(encodedWithEncoding, encoded);
        }
        [Fact]
        public void UrlEncode_should_return_null_if_input_is_null()
        {
            var url = default(string);
            var encoded = url.UrlEncode();
            var encodedWithEncoding = url.UrlEncode(Encoding.GetEncoding(EncodingNames.UTF8));
            Assert.Null(encoded);
            Assert.Null(encodedWithEncoding);
        }

        [Fact]
        public void EncodeChineseChars_should_encode_chinse_normal()
        {
            var ch_a = "我的中国";
            var ch_b = "心痛啊";

            var url = string.Format("http://ou.com/{0}/test/{1}/test.jpg", ch_a, ch_b);

            var ret = url.EncodeChineseChars();

            var expect = string.Format("http://ou.com/{0}/test/{1}/test.jpg", ch_a.UrlEncode(), ch_b.UrlEncode());

            Assert.Equal(expect, ret);
        }

        [Fact]
        public void EncodeChineseChars_should_encode_chinse_if_mixed_and_space()
        {
            var ch_a = "我的中国";
            var ch_b = "心痛啊";
            var space = " ";

            var url = string.Format("http://ou.com/{0}abcdefsgjfda/test{1}fuckfuckfuckyou/test{2}a.jpg", ch_a, ch_b, space);

            var ret = url.EncodeChineseChars();

            var expect = string.Format("http://ou.com/{0}abcdefsgjfda/test{1}fuckfuckfuckyou/test{2}a.jpg", ch_a.UrlEncode(), ch_b.UrlEncode(), space.UrlEncode());

            Assert.Equal(expect, ret);
        }

        [Fact]
        public void EncodeChineseChars_should_encode_symbol()
        {
            var ch_a = "：，！￥《";
            var url = string.Format("http://ou.com/{0}abcdefsgjfda/test{0}fuckfuckfuckyou/testa.jpg", ch_a); ;
            var ret = url.EncodeChineseChars();

            var expect = string.Format("http://ou.com/{0}abcdefsgjfda/test{0}fuckfuckfuckyou/testa.jpg", ch_a.UrlEncode()); ;

            Assert.Equal(expect, ret);
        }

        [Fact]
        public void EncodeChineseChars_should_remain_the_origin_if_no_chinese_chars()
        {
            var url = "http://ou.com/abcdefsgjfda/test/testa.jpg";

            var ret = url.EncodeChineseChars();

            Assert.Equal(url, ret);
        }

        //[Fact]
        //public void MobileFormat_should_remove_86_such_prefix()
        //{
        //    Assert.Equal("13501234578", "+8613501234578".MobileFormat());
        //    Assert.Equal("13501234578", "8613501234578".MobileFormat());
        //    Assert.Equal("13501234578", "08613501234578".MobileFormat());
        //    Assert.Equal("13501234578", "008613501234578".MobileFormat());
        //    Assert.Equal("1065879855", "1065879855".MobileFormat());
        //    Assert.Equal("95555", "95555".MobileFormat());
        //    Assert.Equal("", "".MobileFormat());
        //    Assert.Equal("", (default(string)).MobileFormat());
        //}

        [Fact]
        public void ToDateTime()
        {
            Assert.Equal(DateTime.MinValue, string.Empty.ToExactDateTime(DateTimeFormat.yyyyMMddHHmmss));
            Assert.Equal(DateTime.MinValue, (default(string)).ToExactDateTime(DateTimeFormat.yyyyMMddHHmmss));
            Assert.Equal(DateTime.MinValue, "abc".ToExactDateTime(DateTimeFormat.yyyyMMddHHmmss));
            Assert.Equal(DateTime.MinValue, "134".ToExactDateTime(DateTimeFormat.yyyyMMddHHmmss));
            Assert.Equal(DateTime.MinValue, "    ".ToExactDateTime(DateTimeFormat.yyyyMMddHHmmss));
            Assert.Equal(new DateTime(2012, 7, 13, 14, 29, 34), "20120713142934".ToExactDateTime(DateTimeFormat.yyyyMMddHHmmss));
            Assert.Equal(new DateTime(2012, 7, 13), "2012- 07-13".ToExactDateTime(DateTimeFormat.COMMON_TO_DAY));
        }

        [Fact]
        public void TakeLength()
        {
            var str = "12345678901234567890";
            Assert.Equal(string.Empty, string.Empty.TakeLength(10));
            Assert.Equal(null, (default(string)).TakeLength(1));
            Assert.Equal(str, str.TakeLength(100));
            Assert.Equal(str.Substring(0, 10), str.TakeLength(10));
            Assert.Equal(str, str.TakeLength(str.Length));
            Assert.Equal(str.Substring(0, 1), str.TakeLength(1));
        }

        [Fact]
        public void TakeLength_China()
        {
            var str = "中文测试中文测试中文测试中文测试中文测试";
            Assert.Equal(string.Empty, string.Empty.TakeLength(10));
            Assert.Equal(null, (default(string)).TakeLength(1));
            Assert.Equal(str, str.TakeLength(100));
            Assert.Equal(str.Substring(0, 10), str.TakeLength(10));
            Assert.Equal(str, str.TakeLength(str.Length));
            Assert.Equal(str.Substring(0, 1), str.TakeLength(1));
        }

        [Fact]
        public void LowerTest()
        {
            var strNotNullOrEmpty = "strNotNullOrEmpty";
            string strNull = null;
            string strEmpty = string.Empty;

            Assert.Equal(strNotNullOrEmpty.ToLower(), strNotNullOrEmpty.Lower());
            Assert.Equal(string.Empty, strNull.Lower());
            Assert.Equal(string.Empty, strEmpty.Lower());
        }

        [Fact]
        public void UpperTest()
        {
            Assert.Equal("aaa".ToUpper(), "aaa".Upper());
            Assert.Equal(string.Empty, (default(string)).Upper());
        }

        [Fact]
        public void ToInt64Test()
        {
            Assert.Equal(0, "".ToInt64());
            Assert.Equal(0, "22d".ToInt64());
            Assert.Equal(0, "a".ToInt64());
            Assert.Equal(0, string.Empty.ToInt64());
            Assert.Equal(0, (default(string)).ToInt64());
            Assert.Equal(333, "333".ToInt64());
            Assert.Equal(0, "1.0".ToInt64());
        }

        [Fact]
        public void ToDoubleTest()
        {
            Assert.Equal(0d, "".ToDouble());
            Assert.Equal(0d, "22d".ToDouble());
            Assert.Equal(0d, "a".ToDouble());
            Assert.Equal(0d, string.Empty.ToDouble());
            Assert.Equal(0d, (default(string)).ToDouble());
            Assert.Equal(333, "333".ToDouble());
            Assert.Equal(1.0, "1.0".ToDouble());
        }

        [Fact]
        public void ToFloatTest()
        {
            Assert.Equal(0f, "".ToFloat());
            Assert.Equal(0f, "22d".ToFloat());
            Assert.Equal(0f, "a".ToFloat());
            Assert.Equal(0f, string.Empty.ToFloat());
            Assert.Equal(0f, (default(string)).ToFloat());
            Assert.Equal(333, "333".ToFloat());
            Assert.Equal(1.0, "1.0".ToFloat());
        }

        [Fact]
        public void ConfigValueTest()
        {
            var key = "IsGod";

            Assert.Equal("false", key.ConfigValue());

            SingletonBase<ConfigurableSet>.Instance[key] = "true";
            Assert.Equal("true", key.ConfigValue());

        }

        [Fact]
        public void StringToBytesTest()
        {
            var str = default(string);

            Assert.Null(str.ToBytes());

            str = "a";
            Assert.Equal(new byte[1] { 97 }, str.ToBytes());
        }
    }
}
