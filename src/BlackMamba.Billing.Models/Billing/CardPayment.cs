using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Models
{
    public enum PaymentCardType
    {
        SZX=1,//     神州行
        UNICOM,//               联通卡
        TELECOM,//                   电信卡
        JUNNET,//         骏网一卡通
        SNDACARD,//     盛大卡
        ZHENGTU,//           征途卡
        QQCARD,//             Q币卡
        JIUYOU,//      久游卡
        YPCARD,//                易宝e卡通
        NETEASE,//                网易卡
        WANMEI,//                   完美卡
        SOHU,//      搜狐卡
        ZONGYOU,//               纵游一卡通
        TIANXIA,//              天下一卡通
        TIANHONG    //                 天宏一卡通
    }


    [Serializable]
    public class CardPayment
    {
        public PaymentCardType CardType { get; set; }

        public long OrderID { get; set; }

        public string OrderNo { get; set; }

        public string CardNo { get; set; }

        public string CardPassword { get; set; }

        private float cardAmount = 0.0f;
        public float CardAmount 
        {
            get 
            {
                return cardAmount == 0.0f ? Amount : cardAmount;
            }
            set
            {
                cardAmount = value;
            }
        }

        public float Amount { get; set; }

        private bool verifyAmount = false;
        public bool VerifyAmount
        {
            get
            {
                return verifyAmount;
            }
            set 
            {
                verifyAmount = value;
            }
        }

        public string ProductName { get; set; }

        public string ProductType { get; set; }

        public string ProductDescription { get; set; }

        public string MerchantExtentionalDescription { get; set; }

        public DateTime RequestDateTime { get; set; }

    }
}
