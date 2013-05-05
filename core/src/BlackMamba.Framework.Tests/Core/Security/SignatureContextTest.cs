using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Moq;
using System.Collections.Specialized;
using BlackMamba.Framework.Core.Security;
using BlackMamba.Framework.Core;

namespace BlackMamba.Framework.Tests.Security
{
    namespace SignatureContextTest
    {
        public class ctor : SignatureContextTestBase
        {
            [Fact]
            public void build_normal()
            {
                RequestRepositoryMock.SetupGet<string>(s => s.RawUrl).Returns(DEFAULT_URL);
                this.Context = new SignatureContext(RequestRepositoryMock.Object);


                Assert.Equal(DEFAULT_URL, this.Context.Url);
                Assert.Equal(SignatureMethod.HMAC_SHA1, this.Context.Method);
                Assert.Equal("4c6f1851f6a7496c86ffac67540ae9f5", this.Context.Secret);
                Assert.NotEmpty(this.Context.Query);
                Assert.Equal("9460009830450729", this.Context.Query["imsi"]);
                Assert.Equal("dsfafjoai34343dsfa", this.Context.ClientSignature);
            }
           
        }

        public class GetSignatureMethod 
        {
            [Fact]
            public void should_return_default_if_not_defined()
            {
                var nvs = new NameValueCollection();
                nvs[SignatureContext.KEY_SIGN_TYPE] = "3333";

                // numbers not defined
                var method = SignatureContext.GetSignatureMethod(nvs);
                Assert.Equal(SignatureMethod.MD5, method);

                // letters
                nvs[SignatureContext.KEY_SIGN_TYPE] = "aaa";
                Assert.Equal(SignatureMethod.MD5, SignatureContext.GetSignatureMethod(nvs));

                // empty
                nvs[SignatureContext.KEY_SIGN_TYPE] = "";
                Assert.Equal(SignatureMethod.MD5, SignatureContext.GetSignatureMethod(nvs));

            }
        }

        public class ClientSignature : SignatureContextTestBase
        {
            [Fact]
            public void should_return_empty_if_not_defined()
            {
                RequestRepositoryMock.SetupGet<string>(s => s.RawUrl).Returns("http://inet.kk570.com/appstores/updatelist");
                this.Context = new SignatureContext(RequestRepositoryMock.Object);

                Assert.Equal(string.Empty, this.Context.ClientSignature);
            }
        }

        public class SortedQuery : SignatureContextTestBase
        {
            [Fact]
            public void should_remove_sign_itself()
            {
                RequestRepositoryMock.SetupGet<string>(s => s.RawUrl).Returns(DEFAULT_URL);

                this.Context = new SignatureContext(RequestRepositoryMock.Object);

                Assert.NotEmpty(this.Context.SortedQuery);
                Assert.False(this.Context.SortedQuery.AllKeys.Contains(SignatureContext.Key_SIGN));
                Assert.Equal(this.Context.Query.AllKeys.Count() - 1, this.Context.SortedQuery.AllKeys.Count());
                
            }

            [Fact]
            public void should_remove_sign_itself_and_ignore_sign_case()
            {
                RequestRepositoryMock.SetupGet<string>(s => s.RawUrl).Returns("http://inet.kk570.com/appstores/updatelist?imsi=343434343&SIGN=fafafafadfa&sign_type=2&app_type=yl");

                this.Context = new SignatureContext(RequestRepositoryMock.Object);

                Assert.NotEmpty(this.Context.SortedQuery);
                Assert.False(this.Context.SortedQuery.AllKeys.Contains("SIGN"));
                Assert.Equal(this.Context.Query.AllKeys.Count() - 1, this.Context.SortedQuery.AllKeys.Count());

            }
        }

        public abstract class SignatureContextTestBase
        {
            protected Mock<IRequestRepository> RequestRepositoryMock;

            protected SignatureContext Context;

            protected const string DEFAULT_URL = "http://inet.kk570.com/appstores/updatelist?imsi=9460009830450729&imei=860742000116331&smsc=+8613010305500&batch=MTK_6252_11b&dh=DH000005&pf=F000001&mpm=MD00001&mod=X9.N8.H8.T8.S8.0112.V5.02&lbyver=1008&tm=2012/3/9%2008:30:49&lcd=240X320&no=&ver=&mcode=3&SIM=1&tcard=1&touch=1&kb=1&gs=1&cap=1&java=0&c=1-1000-24M&lua=0&lbs=460:00:14145:26494&istest=1&sign=dsfafjoai34343dsfa&sign_type=2&app_type=yl&keyword=%B3%B1%CA%AA%B5%C4%D0%C4";

            public SignatureContextTestBase()
            {
                RequestRepositoryMock = new Mock<IRequestRepository>();
                //this.Context = new SignatureContext(RequestRepositoryMock.Object);
            }
        }
    }
}
