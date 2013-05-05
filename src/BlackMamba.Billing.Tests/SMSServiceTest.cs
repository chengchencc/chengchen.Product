using BlackMamba.Billing.Domain;
using BlackMamba.Billing.Domain.Common;
using BlackMamba.Billing.Domain.Services;
using BlackMamba.Billing.Domain.Services.SMS;
using BlackMamba.Billing.Models;
using BlackMamba.Billing.Models.SMS;
using BlackMamba.Framework.RedisMapper;
using Moq;
using SubSonic.Oracle.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml;
using Xunit;

namespace BlackMamba.Billing.Tests
{
    public class SMSServiceTest
    {
        public SMSService SMSService { get; set; }
        public Mock<IRepository> MockDbContext { get; set; }
        public Mock<IMailService> MockPaymentsService { get; set; }
        public Mock<IRESTfulClient> MockRESTfulClient { get; set; }
        public Mock<IRedisService> MockRedisService { get; set; }
        const string IMSI_EXAMPLE = "12145785421";

        public SMSServiceTest()
        {
            MockDbContext = new Mock<IRepository>();
            MockPaymentsService = new Mock<IMailService>();
            MockRESTfulClient = new Mock<IRESTfulClient>();
            MockRedisService = new Mock<IRedisService>();

            SMSService = new SMSService(MockRESTfulClient.Object, MockPaymentsService.Object, MockRedisService.Object);
            SMSService.DbContext = MockDbContext.Object;
        }

        [Fact]
        public void IMSIInfo_to_string_test()
        {
            IMSIInfo imsi = new IMSIInfo() { Id = 1, Mobile = "18660127508", IMSI = IMSI_EXAMPLE };

            Assert.Equal("IMSI:12145785421,Mobile:18660127508;", imsi.ToString());
        }

        [Fact]
        public void IMSIInfoList_to_string_test()
        {
            var imsiList = new List<IMSIInfo>();
            imsiList.Add(new IMSIInfo() { Id = 1, Mobile = "18660127508", IMSI = IMSI_EXAMPLE });
            imsiList.Add(new IMSIInfo() { Id = 2, Mobile = "18660127509", IMSI = "12145785422" });
            imsiList.Add(new IMSIInfo() { Id = 3, Mobile = "18660127510", IMSI = "12145785423" });
            imsiList.Add(new IMSIInfo() { Id = 4, Mobile = "18660127511", IMSI = "12145785424" });
            imsiList.Add(new IMSIInfo() { Id = 5, Mobile = "18660127512", IMSI = "12145785425" });

            var result = imsiList.ListToString();
            Assert.Equal("IMSI:12145785421,Mobile:18660127508;IMSI:12145785422,Mobile:18660127509;IMSI:12145785423,Mobile:18660127510;IMSI:12145785424,Mobile:18660127511;IMSI:12145785425,Mobile:18660127512;", result);
        }

        [Fact]
        public void GetIMSIInfo_test()
        {
            IMSIInfo imsiInfo = new IMSIInfo() { Id = 1, Mobile = "18660127508", IMSI = "12145785421" };
            MockDbContext.Setup(s => s.Single<IMSIInfo>(It.IsAny<Expression<Func<IMSIInfo, bool>>>())).Returns(imsiInfo);
            var result = SMSService.GetMobile(IMSI_EXAMPLE);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void GetIMSIInfo_test_with_null_result()
        {
            MockDbContext.Setup(s => s.Single<IMSIInfo>(It.IsAny<Expression<Func<IMSIInfo, bool>>>()));
            var result = SMSService.GetMobile(IMSI_EXAMPLE);
            Assert.Null(result);
        }

        //[Fact]
        //public void GetMobileInfoByPhoneNumber_test()
        //{
        //    IRepository repository = new SimpleRepository(ConnectionStrings.Key_ORACLE_LOG, SimpleRepositoryOptions.None);

        //    //var mobile = SMSService.GetMobileInfoByPhoneNumber("");

        //    var citys = repository.All<City>();
        //    var province = repository.All<Province>();



        //}

       // [Fact]
        public void SMSCallbackLogicTest()
        {
            //IRepository repository = new SimpleRepository(ConnectionStrings.KEY_ORACLE_GENERAL, SimpleRepositoryOptions.RunMigrations);
            //repository.Add<Partner>(new Partner() { CallbackURL = "http://fh_charge.auroraorbit.com/sms_callback.ashx", PartnerNo = "1" });

            IRedisService realRedisService = new RedisService();
            PaymentNotification pn = new PaymentNotification()
            {
                OutOrderNo = "outOrderNo",
                CallbackURL = "http://fh_charge.auroraorbit.com/sms_callback.ashx",
                OrderNo = "orderNo",
                ResultCode = 2
            };
            //realRedisService.AddItemToQueue<PaymentNotification>();
            realRedisService.AddItemToQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE, pn);

        }

        #region SMS Log test
        [Fact]
        public void UpdateSMSLog_Input_smsStatus_smsContent()
        {
            long logId = 11;
            SMSChargeStatus chargeStatus = SMSChargeStatus.Sent;
            string content = "sms message";
            SMSDirection smsDirection = SMSDirection.Receive;

            SMSChannelLog smsChannelLog = new SMSChannelLog
            {
                ID = 11,
                ChargeStatus = SMSChargeStatus.Initial
            };
            MockDbContext.Setup(s => s.Single<SMSChannelLog>(It.IsAny<Expression<Func<IMSIInfo, bool>>>())).Returns(smsChannelLog);
            MockDbContext.Setup(s => s.Update<SMSChannelLog>(It.IsAny<SMSChannelLog>())).Returns(1);
            MockDbContext.Setup(s => s.Add<SMSLog>(It.IsAny<SMSLog>())).Returns(1);
            this.SMSService.UpdateSMSLog(logId, chargeStatus, content, smsDirection, false, "", "", "", "");
        }
        #endregion


    }
}
