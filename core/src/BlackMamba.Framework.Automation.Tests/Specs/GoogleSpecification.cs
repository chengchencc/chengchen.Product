using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watcher.Core.Entity;
using BlackMamba.Framework.Automation;

namespace BlackMamba.Framework.Automation.Tests.Specs
{
    public class GoogleSpecification : SpecificationBase
    {
        public override string ModuleName
        {
            get
            {
                return "Test";
            }
        }

        [Spec]
        public SpecItem should_contain_function_text()
        {
            var item = new SpecItem();
            item.Name = "google";
            item.FixtureType = FixtureType.Html;
            item.Hosts = new List<SpecHost>();

            item.Results = new List<Result>();
            item.Results.Add(new Result { Value = "baidu", DataType = ResultType.String, Opertaion = OP.Include });

            item.Hosts.Add(new SpecHost
            {
                Name = "http://www.baidu.com",
                Port = 80,
            });

            return item;
        }


        [Spec(false)]
        public SpecItem Under_watching_is_false()
        {
            var item = new SpecItem();
            item.Name = "google";
            item.FixtureType = FixtureType.Html;
            item.Hosts = new List<SpecHost>();

            item.Results = new List<Result>();
            item.Results.Add(new Result { Value = "(function()", DataType = ResultType.String, Opertaion = OP.Include });

            item.Hosts.Add(new SpecHost
            {
                Name = "http://www.baidu.com",
                Port = 80,
            });

            return item;
        }
    }
}
