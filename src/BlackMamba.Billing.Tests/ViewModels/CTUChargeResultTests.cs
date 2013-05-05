using BlackMamba.Billing.Domain.ViewModels.SMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace BlackMamba.Billing.Tests.ViewModels
{
    public class CTUChargeResultTests
    {
        [Fact]
        public void should_return_Xml_like_string()
        {
            var ret = new CTUChargeResult();
            ret.Result = CTUChargeResultCodes.InvalidIPAddress;

            Assert.Equal(string.Format("<Result>{0}</Result>", (int)ret.Result), ret.ToViewModelString());
        }
    }
}
