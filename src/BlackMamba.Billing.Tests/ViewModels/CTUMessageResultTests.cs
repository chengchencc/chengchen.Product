using BlackMamba.Billing.Domain.ViewModels.SMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace BlackMamba.Billing.Tests.ViewModels
{
    public class CTUMessageResultTests
    {
        [Fact]
        public void ToString_should_get_one_if_success()
        {
            var ret = new CTUMessageResult();
            ret.Result = CTUMessageResultCodes.Success;


            Assert.Equal("<Result>1</Result>", ret.ToViewModelString());
        }

        [Fact]
        public void ToString_should_get_negtive_if_not_success()
        {
            var ret = new CTUMessageResult();
            ret.Result = CTUMessageResultCodes.ServerIPAuthenticationFailed;


            Assert.Equal("<Result>-16</Result>", ret.ToViewModelString());
        }
    }
}
