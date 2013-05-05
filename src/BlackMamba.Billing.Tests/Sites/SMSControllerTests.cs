using BlackMamba.Billing.Domain.Services.SMS;
using BlackMamba.Billing.Domain.ViewModels.SMS;
using BlackMamba.Billing.Website.Controllers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace BlackMamba.Billing.Tests.Sites
{
    namespace SMSControllerTest
    {
        public class SMSControllerTestBase : TestBase
        {
            protected SMSController Controller { get; set; }

            protected Mock<ISMSService> MockSMSService;

            public SMSControllerTestBase()
            {
                this.MockSMSService = new Mock<ISMSService>();

                this.Controller = new SMSController(this.MockSMSService.Object);
            }

        }

        public class ReceiveTest : SMSControllerTestBase
        {
            const string mobile = "12345600";
            const string msg = "testtesttests";
            const string msg_id = "2323";

            [Fact]
            public void should_get_success_if_everything_is_ok()
            {
                this.MockSMSService.Setup(x => x.CTUSMSReceive(mobile, msg, msg_id)).Returns(new CTUMessageResult());

                var ret = this.Controller.Receive(mobile, msg, msg_id);

                Assert.Equal("<Result>1</Result>", ret.ContentResultData());
            }

            [Fact]
            public void should_get_default_if_exception_occur()
            {
                this.MockSMSService.Setup(x => x.CTUSMSReceive(mobile, msg, msg_id)).Throws(new Exception());

                var ret = this.Controller.Receive(mobile, msg, msg_id);

                Assert.Equal("<Result>-99</Result>", ret.ContentResultData());
            }
        }

        public class NotifyTest : SMSControllerTestBase
        {
            const string order_id = "1";
            const string mobile = "123";
            const string amount = "11";
            const string key = "dfajfkal";
            [Fact]
            public void should_get_success_if_everything_is_ok()
            {
                this.MockSMSService.Setup(x => x.CTUSMSPayCallback(order_id, mobile, amount, key)).Returns(new CTUChargeResult());

                var ret = this.Controller.Notify(order_id, mobile, amount, key);

                Assert.Equal("<Result>0</Result>", ret.ContentResultData());
            }
        }

        public class GetMobilesTest : SMSControllerTestBase
        {
            [Fact]
            public void example()
            {
                var imsi = new List<string> { "23232", "34e3434" };
                var imsis = new List<IMSIInfoViewModel>{new IMSIInfoViewModel{IMSI="23232", Mobile="132323232323"}, 
                    new IMSIInfoViewModel{IMSI="34e3434", Mobile="3249834938"}};

                this.MockSMSService.Setup(x => x.GetMobiles(imsi)).Returns(imsis);

                var ret = this.Controller.GetMobiles(imsi);

                Assert.True(ret.IsContentActionResult());
                Assert.Contains("{\"imsi\":\"23232\",\"mobile\":\"132323232323\"},{\"imsi\":\"34e3434\",\"mobile\":\"3249834938\"}",
                    ret.ContentResultData());
            }
        }
    }
}
