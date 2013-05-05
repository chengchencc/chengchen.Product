using BlackMamba.Billing.Domain.ViewModels.SMS;
using BlackMamba.Billing.Models;
using BlackMamba.Billing.Models.SMS;
using BlackMamba.Framework.SubSonic.Oracle;
using System;
using System.Collections.Generic;
namespace BlackMamba.Billing.Domain.Services.SMS
{
    public interface ISMSService : IDbContext
    {
        MobileInfo GetMobileInfoByPhoneNumber(string phoneNumber);
        City GetCity(int id);
        List<City> GetCityByProvinceId(int provinceId);
        Province GetProvince(int id);
        IMSIInfo GetMobile(string imsi);
        List<IMSIInfoViewModel> GetMobiles(IEnumerable<string> imsiList);
        ChannelResult QueryChannel(string imsi, string mobile, ServiceType serviceType, float? amount, string userNo);
        //ChannelResult CTUSMSPayRequest(float amount, string userNo, string phoneNo);
        CTUMessageResult CTUSMSReceive(string mobile, string msg, string msg_id);
        CTUChargeResult CTUSMSPayCallback(string orderid, string mobile, string Amount, string key);

        void UpdateSMSLog(long logId, SMSChargeStatus chargeStatus, string content, SMSDirection smsDirection, bool isSent, string targetPhoneNo, string orderNo, string outOrderNo, string partenerNo);
    }
}
