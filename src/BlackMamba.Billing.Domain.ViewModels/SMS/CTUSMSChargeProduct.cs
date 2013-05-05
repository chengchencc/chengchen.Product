using BlackMamba.Billing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Domain.ViewModels.SMS
{


    public class CTUSMSChargeProduct 
    {
        public float Amount { get; set; }

        public string SID { get; set; }

        public string RequestKey { get; set; }

        public string ResponseKey { get; set; }
    }
}
