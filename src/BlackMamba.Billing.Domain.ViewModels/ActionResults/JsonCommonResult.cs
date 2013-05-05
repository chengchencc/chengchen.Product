using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace BlackMamba.Billing.Domain.ViewModels
{
    public class JsonCommonResult : JsonResultBase
    {
        public JsonCommonResult(CommonActionResult commonActionResult)
            : base(commonActionResult)
        {
            this.Data = commonActionResult.ViewModels;

            if (this.Data != null) { this.Count = commonActionResult.ViewModels.Count; }

            this.Total = commonActionResult.Total;

            if (this.Total.HasValue)
            {
                this.CustomResultHeaders.Add(new CustomHeaderItem { Key = "total", Value = this.Total.ToString(), IsValueType = true });
            }
        }

        [JsonProperty(PropertyName = "count", Order = 20)]
        public int Count { get; set; }

        [JsonIgnore]
        public int? Total { get; set; }

        [JsonProperty(PropertyName = "data", Order = 100)]
        public IList<IViewModel> Data { get; set; }
    }
}
