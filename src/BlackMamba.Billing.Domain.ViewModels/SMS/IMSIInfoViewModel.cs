using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Domain.ViewModels.SMS
{
    public class IMSIInfoViewModel : ViewModelBase
    {
        [JsonProperty("imsi")]
        public string IMSI { get; set; }

        [JsonProperty("mobile")]
        public string Mobile { get; set; }
    }
}
