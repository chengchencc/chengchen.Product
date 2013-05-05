using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Watcher.Core;
using Watcher.Core.Interface;
using BlackMamba.Framework.Automation;

namespace BlackMamba.Framework.Automation.Tests
{
    public class TestBase : IUseFixture<TestBase>
    {
        public void SetFixture(TestBase data)
        {
            Bootstrapper.Start();

            Bootstrapper.Inject<IDisplay>(new ConsoleDisplay());

        }
    }
}
