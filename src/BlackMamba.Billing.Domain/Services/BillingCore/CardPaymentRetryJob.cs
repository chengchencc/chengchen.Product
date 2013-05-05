using System;
using Quartz;
using BlackMamba.Framework.RedisMapper;
using StructureMap;
using BlackMamba.Billing.Domain.Services;

namespace BlackMamba.Billing.Domain
{
    [DisallowConcurrentExecution] 
    public class CardPaymentRetryJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                CardPaymentProcessor cardPaymentProcessor = new CardPaymentProcessor(ObjectFactory.GetInstance<IMailService>(), ObjectFactory.GetInstance<IRedisService>(),ObjectFactory.GetInstance<IRESTfulClient>());

                int retryCount = context.JobDetail.JobDataMap.GetInt("retryCount");

               // LogHelper.WriteInfo("Retring Card Payment, retry count : " + retryCount);
                
                cardPaymentProcessor.RetrySendCardPaymentRequestToYP(retryCount);
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
            }
        }
    }
}
