using BlackMamba.Framework.Core;
using BlackMamba.Billing.Domain.Services;
using BlackMamba.Framework.RedisMapper;
using StructureMap;
using StructureMap.Configuration.DSL;
using BlackMamba.Billing.Domain.Services.SMS;

namespace BlackMamba.Billing.Domain
{
    public class ControllerRegistry : Registry
    {
        public const string USE_REAL_YEEPAY = "UseRealYeePay";
        public ControllerRegistry()
        {
            RegisterCommonServices();
            RegisterBillingService();
        }

        private void RegisterBillingService()
        {
            if (USE_REAL_YEEPAY.ConfigValue().ToBoolean())
            {
                For<IYeepayService>().Use<YeepayService>();
            }
            else
            {
                For<IYeepayService>().Use<YeepayFakeService>();
            }
        }

        private void RegisterCommonServices()
        {
            For<IRedisService>().Use<RedisService>();

            For<IRESTfulClient>().Use<RESTfulClient>();

            For<IPaymentsUIService>().Use<PaymentsUIService>();

            For<ICardPaymentProcessor>().Use<CardPaymentProcessor>();

            For<IPaymentsService>().Use<PaymentsService>();

            For<IRequestRepository>().Use<RequestRepository>();

            For<ISMSService>().Use<SMSService>();

            For<ISMSUIService>().Use<SMSUIService>();

        }

        #region Debug

        private void RegisterInDebugMode()
        {
        }

        #endregion

        #region Release
        private void RegisterInReleaseMode()
        {
        }
        #endregion

        #region Live

        private void RegisterInLiveMode()
        {
        }
        #endregion

    }
}
