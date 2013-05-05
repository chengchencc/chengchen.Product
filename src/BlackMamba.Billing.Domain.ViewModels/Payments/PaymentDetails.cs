using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BlackMamba.Billing.Domain.ViewModels.Billing
{
    [Serializable]
    public class PaymentDetails : ViewModelBase
    {
        [JsonProperty("orderno")]
        public string OrderNo { get; set; }

        [JsonProperty("status")]
        public string PaymentStatus { get; set; }

        [JsonProperty("payno")]
        public string PaymentNo { get; set; }

        [JsonProperty("errmsg")]
        public string ErrMsg { get; set; }

        [JsonProperty("paydate")]
        public DateTime? PaymentDate { get; set; }

        [JsonProperty("rtype")]
        public string ResponseType { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }
    }
}
