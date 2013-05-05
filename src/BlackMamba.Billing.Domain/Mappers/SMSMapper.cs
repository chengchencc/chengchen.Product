using AutoMapper;
using BlackMamba.Billing.Domain.ViewModels.SMS;
using BlackMamba.Billing.Models.SMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Billing.Domain.Mappers
{
    public class SMSMapper
    {
        internal static void CreateMap()
        {
            Mapper.CreateMap<IMSIInfo, IMSIInfoViewModel>();
        }
    }
}
