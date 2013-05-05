using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Domain.Services
{
    public interface IEMailService
    {
        void SendMail(string strSmtpServer, string strFrom, string strFromPass, string strTo, string strSubject, string strBody);
    }
}
