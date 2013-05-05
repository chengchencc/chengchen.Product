using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watcher.Core.Entity;
using Xunit;
using BlackMamba.Framework.Automation;

namespace BlackMamba.Framework.Automation.Tests
{
    /// <summary>
    /// this class could not be run under teamcity
    /// I do not know how to catch the exception in the xunit fact
    /// </summary>
    public class SpecFactTests : TestBase
    {

        [Spec]
        public SpecItem should_pass_if_passed_result_check()
        {
            var item = new SpecItem();
            item.Name = "app.ff501";
            item.FixtureType = FixtureType.Html;
            item.Hosts = new List<SpecHost>();

            item.Results = new List<Result>();
            item.Results.Add(new Result { Value = "username", DataType = ResultType.String, Opertaion = OP.Include });

            item.Hosts.Add(new SpecHost
            {
                Name = "http://app.ff501.com",
                Port = 80,
            });

            return item;
        }

        [Fact(Skip = "todo")]
        public SpecItem should_throw_exception_if_the_check_failed()
        {
            var item = new SpecItem();
            item.Name = "app.ff501";
            item.FixtureType = FixtureType.Html;
            item.Hosts = new List<SpecHost>();

            item.Results = new List<Result>();
            item.Results.Add(new Result { Value = "username333", DataType = ResultType.String, Opertaion = OP.Include });

            item.Hosts.Add(new SpecHost
            {
                Name = "http://app.ff501.com",
                Port = 80,
            });

            return item;
        }
    }
}
