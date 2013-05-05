using BlackMamba.Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BlackMamba.Framework.Tests.Core
{
    public class RegexHelperTests
    {
        const string PATTERN = @"(?<code>\d+)\w+";
        const string GROUP = "code";

        [Fact]
        public void group_should_return_empty_if_not_matched()
        {
            var ret = RegexHelper.GetMatchedGroupValue("test", PATTERN, GROUP);

            Assert.Equal(string.Empty, ret);
        }

        [Fact]
        public void group_should_return_empty_if_group_not_exist()
        {
            var ret = RegexHelper.GetMatchedGroupValue("111test", PATTERN, "code2");

            Assert.Equal(string.Empty, ret);
        }

        [Fact]
        public void group_should_return_correct_if_matched()
        {
            var ret = RegexHelper.GetMatchedGroupValue("111test", PATTERN, "code");

            Assert.Equal("111", ret);
        }

        [Fact]
        public void group_should_return_empty_if_no_any_group_exist()
        {
            var ret = RegexHelper.GetMatchedGroupValue("111test", @"\d+\w+", "code");

            Assert.Equal(string.Empty, ret);
        }
    }
}
