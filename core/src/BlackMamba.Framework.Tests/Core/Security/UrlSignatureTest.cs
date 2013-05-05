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
    public class UrlSignatureTest
    {
        protected const string DEFAULT_URL = "http://inet.kk570.com/appstores/updatelist?imsi=9460009830450729&imei=860742000116331&smsc=+8613010305500&batch=MTK_6252_11b&dh=DH000005&pf=F000001&mpm=MD00001&mod=X9.N8.H8.T8.S8.0112.V5.02&lbyver=1008&tm=2012/3/9%2008:30:49&lcd=240X320&no=&ver=&mcode=3&SIM=1&tcard=1&touch=1&kb=1&gs=1&cap=1&java=0&c=1-1000-24M&lua=0&lbs=460:00:14145:26494&istest=1&sign=dsfafjoai34343dsfa&sign_type=2&app_type=yl";
        Mock<IRequestRepository> RequestRepositoryMock;
        Mock<ISignature> SignatureMock;

        public UrlSignatureTest()
        {
            RequestRepositoryMock = new Mock<IRequestRepository>();

            SignatureMock = new Mock<ISignature>();
        }

        [Fact]
        public void should_return_false_if_sig_diff_from_client()
        {
            this.RequestRepositoryMock.SetupGet<string>(s => s.RawUrl).Returns(DEFAULT_URL);
            this.SignatureMock.Setup(s => s.Sign(It.IsAny<SignatureContext>())).Returns("232323");
            var urlSignature = new UrlSignature(this.RequestRepositoryMock.Object, Encoding.UTF8);

            urlSignature.Signature = SignatureMock.Object;

            var isValid = urlSignature.IsValid();

            Assert.False(isValid);
        }

        [Fact]
        public void should_return_false_if_secret_is_empty()
        {
            this.RequestRepositoryMock.SetupGet<string>(s => s.RawUrl).Returns(DEFAULT_URL);
            var urlSignature = new UrlSignature(this.RequestRepositoryMock.Object, Encoding.UTF8);

            urlSignature.Context.Secret = string.Empty;

            var isValid = urlSignature.IsValid();

            Assert.False(isValid);
        }

        [Fact]
        public void should_return_true_if_sig_same_with_client()
        {
            this.RequestRepositoryMock.SetupGet<string>(s => s.RawUrl).Returns(DEFAULT_URL);
            this.SignatureMock.Setup(s => s.Sign(It.IsAny<SignatureContext>())).Returns("dsfafjoai34343dsfa");
            var urlSignature = new UrlSignature(this.RequestRepositoryMock.Object, Encoding.UTF8);

            urlSignature.Signature = SignatureMock.Object;

            var isValid = urlSignature.IsValid();

            Assert.True(isValid);
        }

        [Fact]
        public void should_return_false_if_the_url_has_no_sign_information()
        {
            const string url = "http://inet.kk570.com/appstores/updatelist?imsi=9460009830450729&imei=860742000116331&smsc=+8613010305500&batch=MTK_6252_11b&dh=DH000005&pf=F000001&mpm=MD00001&mod=X9.N8.H8.T8.S8.0112.V5.02&lbyver=1008&tm=2012/3/9%2008:30:49&lcd=240X320&no=&ver=&mcode=3&SIM=1&tcard=1&touch=1&kb=1&gs=1&cap=1&java=0&c=1-1000-24M&lua=0&lbs=460:00:14145:26494&istest=1";

            this.RequestRepositoryMock.SetupGet<string>(s => s.RawUrl).Returns(url);
            this.SignatureMock.Setup(s => s.Sign(It.IsAny<SignatureContext>())).Returns("dsfafjoai34343dsfa");
            var urlSignature = new UrlSignature(this.RequestRepositoryMock.Object, Encoding.UTF8);

            urlSignature.Signature = SignatureMock.Object;

            var isValid = urlSignature.IsValid();

            Assert.False(isValid);
        }

        [Fact]
        public void should_be_same_for_the_two_sign_method()
        {
            const string url = "http://inet.kk570.com/appstores/updatelist?imsi=9460009830450729&imei=860742000116331&smsc=+8613010305500&batch=MTK_6252_11b&dh=DH000005&pf=F000001&mpm=MD00001&mod=X9.N8.H8.T8.S8.0112.V5.02&lbyver=1008&tm=2012/3/9%2008:30:49&lcd=240X320&no=&ver=&mcode=3&SIM=1&tcard=1&touch=1&kb=1&gs=1&cap=1&java=0&c=1-1000-24M&lua=0&lbs=460:00:14145:26494&istest=1";

            this.RequestRepositoryMock.SetupGet<string>(s => s.RawUrl).Returns(url);
            this.SignatureMock.Setup(s => s.Sign(It.IsAny<SignatureContext>())).Returns("dsfafjoai34343dsfa");
            var urlSignature = new UrlSignature(this.RequestRepositoryMock.Object, Encoding.UTF8);

            urlSignature.Signature = SignatureMock.Object;

            var sign1 = urlSignature.Sign();
            var sign2 = urlSignature.Sign(new SignatureContext(this.RequestRepositoryMock.Object, Encoding.UTF8));

            Assert.Equal(sign1, sign2);
        }
    }
}
