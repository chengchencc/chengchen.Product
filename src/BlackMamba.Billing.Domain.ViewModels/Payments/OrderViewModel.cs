using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BlackMamba.Billing.Domain.ViewModels.Billing
{
    [Serializable]
    public class OrderViewModel : ViewModelBase
    {
        [JsonProperty("orderno")]
        public string OrderNo { get; set; }
    }
}
