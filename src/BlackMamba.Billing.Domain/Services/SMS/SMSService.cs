using BlackMamba.Billing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackMamba.Framework.SubSonic.Oracle;
using BlackMamba.Billing.Domain.Common;
using SubSonic.Oracle.Repository;
using System.Text.RegularExpressions;
using BlackMamba.Framework.Core;
using BlackMamba.Billing.Models.Payments;
using System.Security.Cryptography;
using NLog;
using System.Xml;
using BlackMamba.Billing.Domain.ViewModels;
using BlackMamba.Billing.Models.SMS;
using BlackMamba.Billing.Domain.ViewModels.SMS;
using BlackMamba.Billing.Domain.Mappers;
using System.Linq.Expressions;
using BlackMamba.Billing.Models.Billing;
using BlackMamba.Framework.RedisMapper;

namespace BlackMamba.Billing.Domain.Services.SMS
{
    public class SMSService : ISMSService
    {
        #region IDbContext
        public string ConnectionStringName
        {
            get
            {
                return ConnectionStrings.KEY_ORACLE_GENERAL;
            }
        }

        public IRepository DbContext
        {
            get
            {
                if (_dbContext == null)
                {
                    _dbContext = DbContextFactory.CreateSimpleRepository(this.ConnectionStringName);
                }

                return _dbContext;
            }
            internal set
            {
                _dbContext = value;
            }
        } private IRepository _dbContext;
        #endregion

        public IRESTfulClient RESTfulCient { get; set; }
        public IMailService PaymentsService { get; set; }
        public IRedisService RedisService { get; set; }
        public IRepository LogTestRepository { get; set; }

        public SMSService(IRESTfulClient restfulCient, IMailService paymentsService, IRedisService redisService)
        {
            this.RESTfulCient = restfulCient;
            PaymentsService = paymentsService;
            RedisService = redisService;
            LogTestRepository = new SimpleRepository(ConnectionStrings.KEY_ORACLE_GENERAL, SimpleRepositoryOptions.None);

            CTUSMSChargeProducts[1f] = new CTUSMSChargeProduct
            {
                Amount = 1f,
                RequestKey = "dkkg99",
                ResponseKey = "ert667",
                SID = "5001"
            };

            CTUSMSChargeProducts[2f] = new CTUSMSChargeProduct
            {
                Amount = 1f,
                RequestKey = "dkf221",
                ResponseKey = "ery332",
                SID = "5002"
            };

            CTUSMSChargeProducts[10f] = new CTUSMSChargeProduct
            {
                Amount = 1f,
                RequestKey = "dfd665",
                ResponseKey = "df2214",
                SID = "5010"
            };
        }

        #region Get Mobile

        /// <summary>
        /// 通过手机号获取手机卡归属地信息
        /// </summary>
        /// <param name="phoneNumber">规则为11位的标准手机号码，或者13位带国际区号如86，或者手机号前七位</param>
        /// <returns></returns>
        public MobileInfo GetMobileInfoByPhoneNumber(string phoneNumber)
        {
            string queryNeeded = string.Empty;
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                if (phoneNumber.Length == 7)
                {
                    queryNeeded = phoneNumber;
                }
                else if (phoneNumber.Length == 11)
                {
                    queryNeeded = phoneNumber.Substring(0, 7);
                }
                else if (phoneNumber.Length == 13)
                {
                    queryNeeded = phoneNumber.Substring(2, 7);
                }
                var result = LogTestRepository.Single<MobileInfo>(s => s.Mobile == queryNeeded);
                return result;
            }
            return null;
        }

        public City GetCity(int id)
        {
            var city = LogTestRepository.Single<City>(s => s.Id == id);
            return city;
        }

        public List<City> GetCityByProvinceId(int provinceId)
        {
            return LogTestRepository.Find<City>(s => s.ProvinceId == provinceId).ToList();
        }

        public Province GetProvince(int id)
        {
            var province = LogTestRepository.Single<Province>(s => s.Id == id);
            return province;
        }

        public ChannelResult QueryChannel(string imsi, string mobile, ServiceType serviceType, float? amount, string userNo)
        {
            ChannelResult result = null;
            List<ShortMessageService> services = new List<ShortMessageService>();
            var mobileInfo = this.GetMobileInfoByPhoneNumber(mobile);
            IList<ShortMessageService> smsServices;

            if (serviceType == ServiceType.SMS)
            {
                smsServices = this.Find<ShortMessageService>(s => s.Type == serviceType);
            }
            else
            {
                if (mobileInfo == null)
                {
                    result = new ChannelResult { Status = ChannelRequestStatus.MissingMobileInfo };
                    return result;
                }
                // filter by operator
                switch (mobileInfo.OperatorId)
                {
                    case 1:
                        smsServices = this.Find<ShortMessageService>(s => s.Type == serviceType && s.IsMobile == true);
                        break;
                    case 2:
                        smsServices = this.Find<ShortMessageService>(s => s.Type == serviceType && s.IsUnicom == true);
                        break;
                    case 3:
                        smsServices = this.Find<ShortMessageService>(s => s.Type == serviceType && s.IsTelcom == true);
                        break;
                    default:
                        smsServices = this.Find<ShortMessageService>(s => s.Type == serviceType);
                        break;
                }
            }


            if (smsServices != null && smsServices.Count > 0)
            {
                services = smsServices.ToList();
                ShortMessageService service = new ShortMessageService();
                Instruction selectedInstruction = new Instruction();
                SMSChannel channel = new SMSChannel();
                ServiceProvider sp = new ServiceProvider();
                for (int i = 0; i < services.Count; i++)
                {
                    service = services[i];
                    sp = this.Single<ServiceProvider>(x => x.Id == service.SpId);

                    // TODO: check channel restriction here

                    if (!service.IsManully)
                    {
                        var instructions = this.Find<Instruction>(s => s.ServiceId == service.Id);
                        channel = this.Single<SMSChannel>(x => x.ServiceId == service.Id);
                        // TODO: check instruction restriction here

                        if (instructions != null && instructions.Count > 0)
                        {
                            selectedInstruction = instructions[0];
                            result = new ChannelResult();
                            result.ServiceNumber = service.ServiceNumber;
                            result.Code = instructions[0].Code;
                            break;
                        }
                    }
                    else
                    {
                        switch (sp.DynamicSP)
                        {
                            case DynamicSP.CTU:
                                return CTUSMSPayRequest(amount.GetValueOrDefault(), userNo, mobileInfo, service.Id, sp.Id, mobile);
                        }
                    }
                }

                if (serviceType == ServiceType.SMSCharge && selectedInstruction != null && channel != null && result != null)
                {
                    // create order
                    var orderNo = PaymentsService.CreateOrder("[短代]", selectedInstruction.Amount, "短信充值：" + selectedInstruction.Amount.ToString(), mobile);
                    result.OrderNo = orderNo;

                    // initial channel log
                    if (mobileInfo == null) mobileInfo = new MobileInfo();
                    SMSChannelLog log = new SMSChannelLog
                    {
                        Amount = selectedInstruction.Amount,
                        ChargeStatus = SMSChargeStatus.Initial,
                        CityId = mobileInfo.CityId,
                        OpId = mobileInfo.OperatorId,
                        ProvinceId = mobileInfo.ProvinceId,
                        ServiceNumber = result.ServiceNumber,
                        Instruction = result.Code,
                        Mobile = mobile,
                        IMSI = GetIMSI(mobile),
                        OrderNo = orderNo
                    };
                    this.AddLog<SMSChannelLog>(log);
                    result.LogId = log.ID;

                    // set channel setting
                    if (channel != null)
                    {
                        var settings = this.Find<SMSChannelSetting>(x => x.ChannelId == channel.Id);
                        if (settings != null && settings.Count > 0)
                        {
                            result.SMSChannelSetting = settings[0];
                        }
                    }

                }
            }
            return result;
        }

        public List<IMSIInfoViewModel> GetMobiles(IEnumerable<string> imsiList)
        {
            var imsiModels = new List<IMSIInfoViewModel>();
            if (imsiList != null)
            {
                foreach (var imsi in imsiList)
                {
                    if (!imsi.IsNullOrEmpty())
                    {
                        var imsiInfo = this.GetMobile(imsi);
                        if (imsiInfo != null)
                        {
                            imsiModels.Add(EntityMapping.Auto<IMSIInfo, IMSIInfoViewModel>(imsiInfo));
                        }
                    }
                }
            }
            return imsiModels;
        }

        public string GetIMSI(string phoneNo)
        {
            var model = this.Single<IMSIInfo>(s => s.Mobile == phoneNo);
            if (model != null)
            {
                return model.IMSI;
            }
            else
            {
                return string.Empty;
            }
        }

        public IMSIInfo GetMobile(string imsi)
        {
            var result = this.Single<IMSIInfo>(s => s.IMSI == imsi);
            return result;
        }
        #endregion

        #region CTU SMS Pay
        /// <summary>
        /// 畅天游短信上行回调接口
        /// </summary>
        const string MSG_PATTERN = @"^XF\+?(?<content>.*)";
        public CTUMessageResult CTUSMSReceive(string mobile, string msg, string msg_id)
        {
            RegexHelper.GetMatchedGroupValue(msg, MSG_PATTERN, "content",
                 x => this.BindPhoneNumber(x, mobile));

            return new CTUMessageResult();
        }

        public void BindPhoneNumber(string imsi, string mobile)
        {
            var imsiEntity = this.Single<IMSIInfo>(x => x.IMSI == imsi);

            if (imsiEntity == null)
            {
                this.Add<IMSIInfo>(new IMSIInfo
                {
                    IMSI = imsi,
                    Mobile = mobile
                });
            }
            else
            {
                imsiEntity.Mobile = mobile;
                imsiEntity.LastModifiedDate = DateTime.Now;
                this.Update<IMSIInfo>(imsiEntity);
            }
        }

        /// <summary>
        /// 畅天游支付成功回调接口
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="mobile"></param>
        /// <param name="Amount"></param>
        /// <param name="key"></param>
        public CTUChargeResult CTUSMSPayCallback(string orderid, string mobile, string Amount, string key)
        {
            #region Update Order Status
            var order = this.Single<Order>(o => o.OrderNo == orderid);
            if (order != null)
            {
                order.Amount = Amount.ToFloat();
                order.CallBackUrl = string.Empty;
                order.OrderStatus = OrderStatus.Complete;
                order.PayedAmount = Amount.ToFloat();
                order.ProductName = "[CTU]短信充值";
                order.UserName = mobile;

                this.Update<Order>(order);
            }

            // create order line
            OrderLine orderLine = new OrderLine
            {
                OrderNo = orderid,
                RealAmount = Amount.ToFloat(),
                PaymentAmount = Amount.ToFloat(),
                PaymentStatus = "SUCCESS",
                Currency = "RMB",
                ProductName = "[CTU]短信充值",
                YeepayPayNo = orderid,
                Price = Amount.ToFloat(),
            };
            this.Add<OrderLine>(orderLine);
            #endregion

            #region Update Channel Log
            var channelLog = this.Single<SMSChannelLog>(x => x.OrderNo == orderid);
            if (channelLog != null)
            {
                if (channelLog.Amount != Amount.ToFloat())
                {
                    LogManager.GetLogger("ErrorLogger")
                        .Error(string.Format("CTUSMSPayCallback amount error: orderid={0} mobile={1} Amount=actual[{2}]/Original[{3}] key={4} channelLogId={5}", orderid, mobile, Amount, channelLog.Amount, key, channelLog.ID));
                }
                channelLog.ChargeStatus = SMSChargeStatus.SPConfirmed;
                this.Update<SMSChannelLog>(channelLog);
            }
            #endregion

            return new CTUChargeResult();
        }

        /// <summary>
        /// 畅天游充值请求接口
        /// </summary>
        Dictionary<float, CTUSMSChargeProduct> CTUSMSChargeProducts = new Dictionary<float, CTUSMSChargeProduct>();
        const string CHARGE_URL = "http://ctucard.800617.com:8002/get.asp?Productsid={0}&orderid={1}&mobile={2}&Amount={3}&key={4}";
        const string CTUChargeRequestRegex = @"^请发送指令(?<instruction>(\w|\s)+)到(?<serviceNo>(\d|\s)+)按照短信提示进行支付$";
        public ChannelResult CTUSMSPayRequest(float amount, string userNo, MobileInfo mobileInfo, int serviceId, int spId, string phoneNo)
        {
            ChannelResult result = new ChannelResult();

            if (!CTUSMSChargeProducts.ContainsKey(amount))
            {
                result.CTURequestStatus = CTURequestStatus.AmountError;
                return result;
            }

            var CTUProductInfo = CTUSMSChargeProducts[amount];

            var orderNo = PaymentsService.CreateOrder("[畅天游]产品ID_" + CTUProductInfo.SID, amount, "短信充值：" + amount.ToString(), userNo, (int)PaymentType.SMS);
            result.OrderNo = orderNo;

            #region Get MD5 key
            string key = orderNo + mobileInfo.Mobile + amount.ToString() + CTUProductInfo.RequestKey;
            byte[] hashedBytes = null;
            using (MD5 md5Hash = MD5.Create())
            {
                hashedBytes = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(key));
            }

            var hashedKey = new StringBuilder();
            for (int i = 0; i < hashedBytes.Length; i++)
            {
                hashedKey.Append(hashedBytes[i].ToString("x2"));
            }
            #endregion

            var url = string.Format(CHARGE_URL, CTUProductInfo.SID, orderNo, mobileInfo.Mobile, amount, hashedKey.ToString());

            var response = this.RESTfulCient.Get(url, 5000);
            if (!string.IsNullOrEmpty(response))
            {
                XmlDocument resultDoc = new XmlDocument();
                try
                {
                    resultDoc.LoadXml(response);
                }
                catch (XmlException ex)
                {
                    LogManager.GetLogger("ErrorLogger").Error(ex.Message);
                    return result;
                }

                var resultNode = resultDoc.SelectSingleNode("/Root/Result");
                var msgNode = resultDoc.SelectSingleNode("/Root/Msg");


                var resultCode = -1;
                int.TryParse(resultNode.InnerText, out resultCode);
                result.CTURequestStatus = (CTURequestStatus)resultCode;
                if (resultCode == 0)
                {
                    result.Code = RegexHelper.GetMatchedGroupValue(msgNode.InnerText, CTUChargeRequestRegex, "instruction");
                    result.ServiceNumber = RegexHelper.GetMatchedGroupValue(msgNode.InnerText, CTUChargeRequestRegex, "serviceNo");

                    SMSChannelLog log = new SMSChannelLog
                    {
                        Amount = amount,
                        ChargeStatus = SMSChargeStatus.Initial,
                        CityId = mobileInfo.CityId,
                        OpId = mobileInfo.OperatorId,
                        ProvinceId = mobileInfo.ProvinceId,
                        ServiceNumber = result.ServiceNumber,
                        Instruction = result.Code,
                        Mobile = phoneNo,
                        IMSI = GetIMSI(phoneNo),
                        OrderNo = orderNo,
                    };

                    if (string.IsNullOrEmpty(result.Code) || string.IsNullOrEmpty(result.ServiceNumber))
                    {
                        result.Status = ChannelRequestStatus.NoChannelFound;
                        log.ChargeStatus = SMSChargeStatus.NotFound;
                    }
                    else  // CTU returned a valid channel
                    {
                        var channel = this.Single<SMSChannel>(x => x.ServiceId == serviceId);
                        if (channel != null)
                        {
                            var smsChannelSetting = this.Single<SMSChannelSetting>(x => x.ChannelId == channel.Id);
                            if (smsChannelSetting != null)
                            {
                                result.SMSChannelSetting = smsChannelSetting;
                            }
                        }
                        log.ChargeStatus = SMSChargeStatus.Initial;
                        // find saved channel
                        //var savedChannel = this.Single<SMSChannel>(x => x.ServiceId == serviceId && x.ServiceNumber == result.ServiceNumber);

                        //// channel not found, just save it
                        //if (savedChannel == null)
                        //{
                        //    ShortMessageService service = new ShortMessageService
                        //    {
                        //        SpId = spId,
                        //        Type = ServiceType.SMSCharge,
                        //        ServiceNumber = result.ServiceNumber,
                        //        Name = string.Format("[Auto]Province[{0}]City[{1}]Op[{2}]",  mobileInfo.ProvinceId, mobileInfo.CityId, mobileInfo.OperatorId),
                        //        IsUnicom = mobileInfo.OperatorId == 2,
                        //        IsMobile = mobileInfo.OperatorId == 1,
                        //        IsTelcom = mobileInfo.OperatorId == 3,


                        //    };
                        //}
                        //else // found saved channel, read and apply its setting and restrictions
                        //{

                        //}

                        // send channel setting info
                        //if (channel != null)
                        //{
                        //    result.SMSChannelSetting = new SMSChannelSetting
                        //    {
                        //        ChannelId = channel.Id,
                        //        ChargeMessageRegex = channel.ChargeMessageRegex,
                        //        ChargeMessageTemplate = channel.ChargeMessageTemplate,
                        //        FinalConfirmMessage = channel.FinalConfirmMessage,
                        //        ConfirmMessage = channel.ConfirmMessage,
                        //        ConfirmMessageTemplate = channel.ConfirmMessageTemplate
                        //    };
                        //}
                    }

                    this.AddLog<SMSChannelLog>(log);
                    result.LogId = log.ID;
                }
                else
                {
                    LogManager.GetLogger("ErrorLogger").Error(CHARGE_URL + "  response:" + response);
                }
            }

            return result;
        }

        #endregion

        #region Channel Management
        public Region GetMobileRegion(string mobile)
        {
            return new Region();
        }
        #endregion

        #region SMS Log
        public void UpdateSMSLog(long logId, SMSChargeStatus chargeStatus, string content, SMSDirection smsDirection, bool isSent, string targetPhoneNo, string orderNo, string outOrderNo, string partenerNo)
        {
            if (chargeStatus != SMSChargeStatus.NotChange)
            {
                var smsChannelLog = this.Single<SMSChannelLog>(s => s.ID == logId);
                if (smsChannelLog != null)
                {
                    smsChannelLog.ChargeStatus = chargeStatus;
                    this.Update<SMSChannelLog>(smsChannelLog, false);
                }
            }

            if (chargeStatus == SMSChargeStatus.Sent && !string.IsNullOrEmpty(orderNo))
            {
                var order = this.Single<Order>(s => s.OrderNo == orderNo);
                if (order != null)
                {
                    order.CardPaymentRequestStatus = CardPaymentRequestStatus.Success;
                    order.OrderStatus = OrderStatus.Processing;
                    order.OutOrderNo = outOrderNo;
                    order.PartenerNo = partenerNo;
                    this.Update<Order>(order, false);
                    SMSCallbackLogic(order);
                }
            }

            if (chargeStatus == SMSChargeStatus.Success && !string.IsNullOrEmpty(orderNo))
            {
                var order = this.Single<Order>(s => s.OrderNo == orderNo);
                if (order != null)
                {
                    order.OrderStatus = OrderStatus.Successed;
                    this.Update<Order>(order, false);
                    SMSCallbackLogic(order);
                }
            }

            if (!string.IsNullOrEmpty(content) && logId != 0)
            {
                var smsLog = new SMSLog()
                {
                    ChannelLogId = logId,
                    Content = content,
                    Direction = smsDirection,
                    IsSent = isSent,
                    TargetPhoneNo = targetPhoneNo,
                };
                this.Add<SMSLog>(smsLog);
            }
        }

        internal void SMSCallbackLogic(Order order)
        {
            if (order.PartenerNo != null)
            {
                var cardPaymentCallbackResult = new PaymentNotification();
                cardPaymentCallbackResult.OrderNo = order.OrderNo;
                cardPaymentCallbackResult.SuccessAmount = order.PayedAmount;
                cardPaymentCallbackResult.RequestAmount = order.Amount;
                cardPaymentCallbackResult.ResultCode = (int)order.OrderStatus;
                cardPaymentCallbackResult.OutOrderNo = order.OutOrderNo;
                var partner = this.Single<Partner>(s => s.PartnerNo == order.PartenerNo);
                if (partner != null)
                {
                    cardPaymentCallbackResult.CallbackURL = partner.CallbackURL;
                    this.RedisService.AddItemToQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE, cardPaymentCallbackResult);
                }
            }
        }


        #endregion

    }

    public static class ToStringExtension
    {
        public static string ListToString(this List<IMSIInfo> input)
        {
            var result = string.Empty;
            foreach (var imsiInfo in input)
            {
                result += imsiInfo.ToString();
            }
            return result;
        }
    }

}
