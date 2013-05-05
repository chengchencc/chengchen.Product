using BlackMamba.Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Framework.Core.Tests.NCore
{
    public class LambdaComparerTests
    {
        [Fact]
        public void should_support_lambda_for_common_objects()
        {
            var pA = new Person { firstname = "jeremy", lastname = "cui", age = 22 };
            var pB = new Person { firstname = "jeremy", lastname = "cui", age = 23 };

            var listA = new List<Person>() { pA };
            var lisbB = new List<Person>() { pB };

            var ret = listA.Intersect<Person>(lisbB, new LambdaComparer<Person>((x, y) => x.firstname.EqualsOrdinalIgnoreCase(y.firstname)));
            Assert.True(ret.Count() == 1);
        }

        [Fact]
        public void hashCode_should_be_the_func_result()
        {
            var compare = new LambdaComparer<Person>((x, y) => x.firstname == y.firstname, z => z.age.GetHashCode());
            var p = new Person {age = 1 };

            Console.WriteLine(compare.GetHashCode(p));
            Assert.Equal(p.age.GetHashCode(), compare.GetHashCode(p));
        }

        public class Person
        {
            public string firstname { get; set; }

            public string lastname { get; set; }

            public int age { get; set; }
        }
    }
}
