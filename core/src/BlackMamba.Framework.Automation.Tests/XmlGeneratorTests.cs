using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;
using BlackMamba.Framework.Automation;

namespace BlackMamba.Framework.Automation.Tests
{
    public class XmlGeneratorTests
    {
        [Fact(Skip="todo")]
        public void can_get_methods()
        {
            var assemblyName = "Integration.Core.Tests.dll";

            var methods = XmlGenerator.GetWatchItemMethods(assemblyName);

            Assert.True(methods.Count > 0);
            Assert.True(methods.Exists(x => x.Name == "should_contain_function_text"));
        }

        [Fact(Skip = "todo")]
        public void should_skip_the_method_whose_under_watching_is_false()
        {
            var assemblyName = "Integration.Core.Tests.dll";

            var methods = XmlGenerator.GetWatchItemMethods(assemblyName, true);

            Assert.False(methods.Exists(x => x.Name == "Under_watching_is_false"));
        }

        [Fact(Skip = "todo")]
        public void can_generate_xml()
        {
            var assemblyName = "Integration.Core.Tests.dll";

            new XmlGenerator().Do(assemblyName);

            Assert.True(File.Exists(XmlGenerator.FILE_NAME));
        }
    }
}
