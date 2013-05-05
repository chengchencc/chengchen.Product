using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using BlackMamba.Framework.Core;

namespace Framework.Core.Tests.NCore.Extensions
{
    public class TypeExtensionsTest
    {
        public class TestList : List<int>
        { }

        public class TestNormal { }

        [Serializable]
        public class Person
        {
            public int Age { get; set; }
        }

        public class PersonList : List<Person> { }

        [Fact]
        public void IsEnumerableType_test()
        {
            Assert.False((default(Type)).IsEnumerableType());

            Assert.True(typeof(TestList).IsEnumerableType());
            Assert.True(typeof(TestList).IsEnumerableType());
            
            Assert.False(typeof(TestNormal).IsEnumerableType());
            Assert.False(typeof(TestNormal).IsEnumerableType());
        }

        [Fact]
        public void IsSerializableEnumerableType_test()
        {
            Assert.False((default(Type)).IsSerializableEnumerableType());

            Assert.True(typeof(PersonList).IsSerializableEnumerableType());
            Assert.True(typeof(PersonList).IsSerializableEnumerableType());

            Assert.False(typeof(TestNormal).IsSerializableEnumerableType());
            Assert.False(typeof(TestNormal).IsSerializableEnumerableType());
        }
    }
}
