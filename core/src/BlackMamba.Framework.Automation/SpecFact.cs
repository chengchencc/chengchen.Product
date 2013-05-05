using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Sdk;

namespace BlackMamba.Framework.Automation
{
    public class SpecAttribute : FactAttribute
    {
        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            var command = new SpecCommand(method, LiveOnly);

            yield return command;
        }

        public SpecAttribute()
        {
            UnderWatching = true;
            LiveOnly = false;
        }

        public SpecAttribute(bool underWatching)
        {
            this.UnderWatching = underWatching;
        }

        public bool UnderWatching { get; set; }

        public bool LiveOnly { get; set; }
    }
}
