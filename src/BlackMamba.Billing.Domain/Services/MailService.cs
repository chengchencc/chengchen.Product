using System;
using System.Collections.Generic;
using System.Linq;
using BlackMamba.Framework.Core;
using System.Text;
using com.yeepay.cmbn;
using com.yeepay.Utils;


using System.Configuration;
using BlackMamba.Billing.Models.Billing;
using System.Net.Mail;
namespace BlackMamba.Billing.Domain.Services
{
    public class EMailService: IEMailService
    {
            /// <summary>
            /// Net 2.0 发送邮件
            /// </summary>
            /// <param name="strSmtpServer"> 发送邮件服务器 </param>
            /// <param name="strFrom"> 发信人Email</param>
            /// <param name="strFromPass"> 发信人Email密码 </param>
            /// <param name="strTo"> 收信人Email</param>
            /// <param name="strSubject"> 邮件主题</param>
            /// <param name="strBody"> 邮件内容</param>
            public  void SendMail(string strSmtpServer, string strFrom, string strFromPass, string strTo, string strSubject, string strBody)
            {
                SmtpClient client = new SmtpClient(strSmtpServer);

                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(strFrom, strFromPass);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                Byte[] b = Encoding.Default.GetBytes(strBody);
                strBody = Encoding.GetEncoding("gb2312").GetString(b).ToString();

                MailMessage message = new MailMessage(strFrom, strTo, strSubject, strBody);

                message.BodyEncoding = Encoding.UTF8;
                message.IsBodyHtml = true;

                client.Send(message);
            }
    
    }
}
