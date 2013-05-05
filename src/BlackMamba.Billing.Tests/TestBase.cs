using BlackMamba.Billing.Domain;
using BlackMamba.Framework.Core;
using Moq;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Xunit;

namespace BlackMamba.Billing.Tests
{
    public class TestBase : IUseFixture<TestBase>
    {
        public void SetFixture(TestBase data)
        {
            Bootstrapper.Start();

            //TestWarmer.Start(ShouldFlushRedisBeforeTest);

            var requestRepository = new Mock<IRequestRepository>();

            ObjectFactory.Inject<IRequestRepository>(requestRepository.Object);

            requestRepository.Setup<NameValueCollection>(s => s.Header).Returns(new NameValueCollection());
        }
    }
}
