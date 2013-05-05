using System;
using System.ServiceProcess;
using System.Threading.Tasks;
using BlackMamba.Framework.Core;
using Quartz;
using Quartz.Impl;
using BlackMamba.Framework.RedisMapper;
using StructureMap;
using BlackMamba.Billing.Domain.Services;
using BlackMamba.Billing.Domain;
using System.Threading;

namespace BlackMamba.Billing.Background
{
    partial class BillingService : ServiceBase
    {
        ISchedulerFactory sf = new StdSchedulerFactory();
        IScheduler sched = null;

        public BillingService()
        {
            InitializeComponent();

            sched = sf.GetScheduler();
        }

        protected override void OnStart(string[] args)
        {
            JobStart();
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            sched.Shutdown(true);
            base.OnStop();
        }

        internal void JobStart()
        {
            Bootstrapper.Start();

            //ScheduleRetryTasks();

            //InitialCardPaymentRequestWorkers();
            CheckIP();
        }

        private void CheckIP()
        {
            while (true)
            {
                try
                {
                    var ipProcessor = new IpProcessor();
                    ipProcessor.Check();
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError(string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
                }
                Thread.Sleep(5000);
            }
        }

        private void InitialCardPaymentRequestWorkers()
        {
            CardPaymentProcessor cardPaymentProcessor = new CardPaymentProcessor(ObjectFactory.GetInstance<IMailService>(), ObjectFactory.GetInstance<IRedisService>(), ObjectFactory.GetInstance<IRESTfulClient>());

            Action<object> q0Action = (object param) =>
            {
                while (true)
                {
                    try
                    {
                        //LogHelper.WriteInfo("Sending Card Payment Request To YP from thread : " + param);
                        cardPaymentProcessor.SendCardPaymentRequestToYP();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteError(string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
                    }
                }
            };

            Action<object> q3Action = (object param) =>
            {
                while (true)
                {
                    try
                    {
                        //LogHelper.WriteInfo("Sending CallBack from thread : " + param, ConsoleColor.Green);
                        cardPaymentProcessor.SendCardPaymentCallBack();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteError(string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
                    }
                }
            };

            for (int i = 0; i < "WorkerNumber".ConfigValue().ToInt32(); i++)
            {
                Task q0Worker = Task.Factory.StartNew(q0Action, i);
                Task q3Worker = Task.Factory.StartNew(q3Action, i);
            }
        }
       
        private void ScheduleRetryTasks()
        {
            var retryCount = "RetryTimes".ConfigValue().ToInt32();
            var retryIntervals = "RetryIntervals".ConfigValue().Split(',');

            for (int i = 0; i < retryCount; i++)
            {
                int intervalSecs = retryIntervals[i].ToInt32() / 5;
                var trigger = (ISimpleTrigger)TriggerBuilder.Create()
                                           .WithIdentity("retryTrigger" + (i + 1), "CardPaymentRetryTriggers")
                                           .StartAt(DateTime.Now)
                                           .WithSimpleSchedule(x => x.WithIntervalInSeconds(intervalSecs).RepeatForever())
                                           .Build();

                var triggerNotify = (ISimpleTrigger)TriggerBuilder.Create()
                                           .WithIdentity("retryNotifyTrigger" + (i + 1), "CardPaymentRetryNotifyTriggers")
                                           .StartAt(DateTime.Now.AddSeconds(2))
                                           .WithSimpleSchedule(x => x.WithIntervalInSeconds(intervalSecs).RepeatForever())
                                           .Build();

                var retryJob = JobBuilder.Create<CardPaymentRetryJob>()
                .WithIdentity("retryJob" + (i + 1), "CardPaymentRetryJobs")
                .UsingJobData("retryCount", i + 1)
                .Build();

                var retryNotifyJob = JobBuilder.Create<CardPaymentRetryNotifyJob>()
                .WithIdentity("retryNotifyJob" + (i + 1), "CardPaymentRetryNotifyJobs")
                .UsingJobData("retryCount", i + 1)
                .Build();

                sched.ScheduleJob(retryNotifyJob, triggerNotify);
                //LogHelper.WriteInfo(string.Format("Scheduled notify retry job {0} with repeat interval : {1}", i + 1, intervalSecs), ConsoleColor.Green);
                sched.ScheduleJob(retryJob, trigger);
                //LogHelper.WriteInfo(string.Format("Scheduled pay request retry job {0} with repeat interval : {1}", i + 1, intervalSecs));
            }

            //var statTrigger = (ICronTrigger)TriggerBuilder.Create()
            //                               .WithIdentity("statTrigger")
            //                               .WithCronSchedule("0 30 0 * * ?")
            //                               //.WithCronSchedule("0/10 * * * * ?")
            //                               .Build();

            //var statJob = JobBuilder.Create<SaveStatisticsJob>()
            //    .WithIdentity("statJob")
            //    .Build();

            //sched.ScheduleJob(statJob, statTrigger);
            
            //ScheduleBaiduExportorTask();
            
            sched.Start();

        }

        //private void ScheduleBaiduExportorTask()
        //{
        //    var statTrigger = (ICronTrigger)TriggerBuilder.Create()
        //                                      .WithIdentity("baiduTrigger")
        //                                      .WithCronSchedule("0 0 0/2 * * ?")
        //                                      //.WithCronSchedule("0 0/1 * 1/1 * ? *")
        //                                      .Build();

        //    var statJob = JobBuilder.Create<BaiduExportorJob>()
        //        .WithIdentity("baiduJob")
        //        .Build();

        //    sched.ScheduleJob(statJob, statTrigger);

        //}
      
    }
}
