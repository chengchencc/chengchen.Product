using BlackMamba.Billing.Domain.Mappers;
using BlackMamba.Billing.Domain.ViewModels.SMS;
using BlackMamba.Billing.Models.SMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace BlackMamba.Billing.Tests.Domains.Mappers
{
    public class SMSMapperTest : TestBase
    {
        [Fact]
        public void should_map_from_imsi_info()
        {
            var imsi = new IMSIInfo() { IMSI = "12343545", Mobile = "223232" };

            var ret = EntityMapping.Auto<IMSIInfo, IMSIInfoViewModel>(imsi);

            Assert.Equal(imsi.IMSI, ret.IMSI);
            Assert.Equal(imsi.Mobile, ret.Mobile);

        }
    }
}
