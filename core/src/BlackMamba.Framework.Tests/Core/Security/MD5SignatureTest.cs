using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Moq;
using BlackMamba.Framework.Core;
using BlackMamba.Framework.Core.Security;

namespace BlackMamba.Framework.Tests.Security
{
    public class MD5SignatureTest
    {
        protected const string DEFAULT_URL = "http://inet.kk570.com/appstores/updatelist?imsi=9460009830450729&imei=860742000116331&smsc=+8613010305500&batch=MTK_6252_11b&dh=DH000005&pf=F000001&mpm=MD00001&mod=X9.N8.H8.T8.S8.0112.V5.02&lbyver=1008&tm=2012/3/9%2008:30:49&lcd=240X320&no=&ver=&mcode=3&SIM=1&tcard=1&touch=1&kb=1&gs=1&cap=1&java=0&c=1-1000-24M&lua=0&lbs=460:00:14145:26494&istest=1&sign=dsfafjoai34343dsfa&sign_type=1&app_type=yl";
        [Fact]
        public void should_equal_with_online_internet()
        {
            var mockRequestRepository = new Mock<IRequestRepository>();

            mockRequestRepository.SetupGet<string>(s => s.RawUrl).Returns(DEFAULT_URL);

            var context = new SignatureContext(mockRequestRepository.Object, Encoding.UTF8);

            var md5 = new MD5Signature();
            var ret = md5.Sign(context);

            Assert.Equal("e78ea96fcecd4a3df8ddf9d776246e6a", ret);
        }

        [Fact]
        public void query_string_is_empty()
        {
            var mockRequestRepository = new Mock<IRequestRepository>();

            mockRequestRepository.SetupGet<string>(s => s.RawUrl).Returns("http://localhost:11944/Misc/EncUtil");

            var context = new SignatureContext(mockRequestRepository.Object, Encoding.UTF8);

            var md5 = new MD5Signature();
            var ret = md5.Sign(context);

            Assert.Equal("d41d8cd98f00b204e9800998ecf8427e", ret);
        }

    }
}
