using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using BlackMamba.Framework.Core;

namespace Framework.Core.Tests.NCore.Extensions
{
    /// <summary>
    /// Todo: added the ncore test here
    /// </summary>
    public class Int32ExtensionsTest
    {

        [Fact]
        public void ToBoolean_Tests()
        {
            Assert.True("true".ToBoolean());
            Assert.True("True ".ToBoolean());
            Assert.True("tRue".ToBoolean());
            Assert.True("TRUE".ToBoolean());
            Assert.False("true2".ToBoolean());
            Assert.True(1.ToBoolean());
            Assert.False(0.ToBoolean());
            Assert.False("False".ToBoolean());
            Assert.False("fAlse".ToBoolean());
            Assert.False("FALSE".ToBoolean());
            Assert.False("false ".ToBoolean());
            Assert.False("1111".ToBoolean());
        }

        [Fact]
        public void ToEnum_test()
        {
            Assert.Equal(ConsoleColor.Black, 0.ToEnum<ConsoleColor>(ConsoleColor.Blue));
            Assert.Equal(ConsoleColor.Blue, 111110.ToEnum<ConsoleColor>(ConsoleColor.Blue));
        }

        [Fact]
        public void GetSize_Test()
        {
            string B = " B";
            string KB = " KB";
            string MB = " MB";
            int size = 0;
            var test = size.ToFileSize();
            Assert.Equal(size + B, test);

            size = 1023;
            test = size.ToFileSize();
            Assert.Equal("1,023" + B, test);

            size = 12034;
            test = size.ToFileSize();
            Assert.Equal("11.75" + KB, test);

            size = 1248576;
            test = size.ToFileSize();
            Assert.Equal("1.19" + MB, test);

            size = -100;
            test = size.ToFileSize();
            Assert.Equal(string.Empty, test);
        }
    }
}
