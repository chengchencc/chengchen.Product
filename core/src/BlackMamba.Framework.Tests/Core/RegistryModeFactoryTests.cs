using BlackMamba.Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Framework.Core.Tests.NCore
{
    public class RegistryModeFactoryTests
    {
        [Fact]
        public void GetCurrentMode_should_return_correct_mode()
        {
            RegistryModeFactory.ClearConditions();
            var mode = RegistryModeFactory.GetCurrentMode();

            Console.WriteLine(mode);

            if (ProjectConfigHelper.IsInDebugMode())
            {
                Assert.Equal(mode, RegistryMode.Debug);
            }

            if (ProjectConfigHelper.IsInReleaseMode())
            {
                Assert.Equal(mode, RegistryMode.Release);
            }

            if (ProjectConfigHelper.IsInLiveMode())
            {
                Assert.Equal(mode, RegistryMode.Live);
            }
        }

        [Fact]
        public void GetCurrentMode_can_be_injected_outside_if_condition_is_single()
        {
            RegistryModeFactory.ClearConditions();
            RegistryModeFactory.AddCondition(() => RegistryMode.Release);

            var mode = RegistryModeFactory.GetCurrentMode();

            Assert.Equal(RegistryMode.Release, mode);
        }


        [Fact]
        public void GetCurrentMode_can_be_injected_outside_if_condition_are_multiple()
        {
            RegistryModeFactory.ClearConditions();
            RegistryModeFactory.AddCondition(() => RegistryMode.Release);
            RegistryModeFactory.AddCondition(() => RegistryMode.Live);

            var mode = RegistryModeFactory.GetCurrentMode();

            Assert.Equal(RegistryMode.Release | RegistryMode.Live, mode);
        }
    }
}
