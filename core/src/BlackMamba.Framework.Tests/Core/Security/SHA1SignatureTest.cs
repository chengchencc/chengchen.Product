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
    public class SHA1SignatureTest
    {
        //64e2dc0913780869f1aebd32aadb8e76aecadf28

        protected const string DEFAULT_URL = "http://inet.kk570.com/appstores/updatelist?imsi=9460009830450729&imei=860742000116331&smsc=+8613010305500&batch=MTK_6252_11b&dh=DH000005&pf=F000001&mpm=MD00001&mod=X9.N8.H8.T8.S8.0112.V5.02&lbyver=1008&tm=2012/3/9%2008:30:49&lcd=240X320&no=&ver=&mcode=3&SIM=1&tcard=1&touch=1&kb=1&gs=1&cap=1&java=0&c=1-1000-24M&lua=0&lbs=460:00:14145:26494&istest=1&sign=dsfafjoai34343dsfa&sign_type=2&app_type=yl";

        /// <summary>
        /// utf-8
        /// </summary>
        [Fact]
        public void should_equal_with_snda_result()
        {
            var mockRequestRepository = new Mock<IRequestRepository>();

            mockRequestRepository.SetupGet<string>(s => s.RawUrl).Returns(DEFAULT_URL);

            var context = new SignatureContext(mockRequestRepository.Object, Encoding.UTF8);

            var sha1 = new SHA1Signature();
            var ret = sha1.Sign(context);

            Assert.Equal("53ed4585d50783c4fa1a0f5605a299622494169f", ret);
        }

        [Fact]
        public void query_string_is_empty()
        {
            var mockRequestRepository = new Mock<IRequestRepository>();

            mockRequestRepository.SetupGet<string>(s => s.RawUrl).Returns("http://localhost:11944/Misc/EncUtil");

            var context = new SignatureContext(mockRequestRepository.Object, Encoding.UTF8);

            var sha1 = new SHA1Signature();
            var ret = sha1.Sign(context);

            Assert.NotEqual("b6a59234783b8ed4afd1cd5d4230bbe0c6de13dd", ret);
        }
    }
}
