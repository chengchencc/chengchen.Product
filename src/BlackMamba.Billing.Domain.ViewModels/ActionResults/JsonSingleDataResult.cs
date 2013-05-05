using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BlackMamba.Billing.Domain.ViewModels
{
    public class JsonSingleDataResult : JsonResultBase
    {
        public JsonSingleDataResult(CommonActionResult commonActionResult)
            : base(commonActionResult)
        {
            if (commonActionResult.ViewModels != null && commonActionResult.ViewModels.Count > 0)
                this.Data = commonActionResult.ViewModels[0];
        }

        [JsonProperty(PropertyName = "data", Order = 100)]
        public IViewModel Data { get; set; }
    }

    /// <summary>
    /// For Deserializing Object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonSingleDataResult<T>
        where T : class, IViewModel
    {
        [JsonProperty(PropertyName = "result", Order = 0)]
        public int ResultCode { get; set; }

        [JsonProperty(PropertyName = "desc", Order = 10)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "host", Order = 20)]
        public string Host { get; set; }

        [JsonProperty(PropertyName = "data", Order = 100)]
        public T Data { get; set; }
    }
}
