using System;
using Quartz;
using BlackMamba.Framework.RedisMapper;
using StructureMap;
using BlackMamba.Billing.Domain.Services;

namespace BlackMamba.Billing.Domain
{
    [DisallowConcurrentExecution] 
    public class CardPaymentRetryNotifyJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                CardPaymentProcessor cardPaymentProcessor = new CardPaymentProcessor(ObjectFactory.GetInstance<IMailService>(), ObjectFactory.GetInstance<IRedisService>(),ObjectFactory.GetInstance<IRESTfulClient>());

                int retryCount = context.JobDetail.JobDataMap.GetInt("retryCount");

                //LogHelper.WriteInfo("RetryNotify, retry count : " + retryCount, ConsoleColor.Green);

                cardPaymentProcessor.RetrySendCardPaymentCallBack(retryCount);
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
            }
        }
    }
}
