
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Watcher.Core.Entity;
using BlackMamba.Framework.Automation;

namespace BlackMamba.Framework.Automation.Tests
{
    public class SpecItemTests
    {
        [Fact]
        public void should_not_add_same_host_with_the_name()
        {
            var spec = new SpecItem();

            var host1 = "http://a.a.com";
            spec.AddHost(host1);
            spec.AddHost(host1);
            spec.AddHost(host1);
            spec.AddHost(host1);

            Assert.Equal(1, spec.Hosts.Count);
        }

        [Fact]
        public void should_not_add_same_parameter()
        {
            var spec = new SpecItem();

            var p = new Param { Name = "a", Value = "b" };

            spec.AddParameter(p.Name, p.Value);
            spec.AddParameter(p.Name, p.Value);
            spec.AddParameter(p.Name, p.Value);
            spec.AddParameter(p.Name, p.Value);

            Assert.Equal(1, spec.Params.Count);
        }

        [Fact]
        public void should_not_add_parameter_if_exist_when_assign_url()
        {
            var spec = new SpecItem();

            var p = new Param { Name = "a", Value = "b" };

            spec.AddParameter(p.Name, p.Value);

            spec.LinkUrl = "/a/b/c?a=c";

            Assert.Equal(1, spec.Params.Count);
        }

        [Fact]
        public void should_add_params_when_assign_link_url()
        {
            var spec = new SpecItem();
            spec.LinkUrl = "/a/b/c?a=c&b=d&mm=ss&d=";

            Assert.Equal(4, spec.Params.Count);
        }

        [Fact]
        public void should_add_host_if_contains_port()
        {
            var spec = new SpecItem();

            var host1 = "http://a.a.com:8080";
            spec.AddHost(host1);

            Assert.Equal(1, spec.Hosts.Count);
        }
    }
}
