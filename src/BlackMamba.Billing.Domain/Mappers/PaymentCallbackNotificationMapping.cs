using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using BlackMamba.Billing.Models;
using BlackMamba.Billing.Models.Payments;

namespace BlackMamba.Billing.Domain.Mappers
{
    public class PaymentCallbackNotificationMapping
    {
        internal static void CreateMap()
        {
            Mapper.CreateMap<PaymentNotification, CallbackNotification>()
                .ForMember(dest => dest.IsNotifySuccess, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());

            Mapper.CreateMap<CardPayment, CardPaymentRetry>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            Mapper.CreateMap<CardPaymentRetry, CardPayment>()
                .ForMember(dest => dest.RequestDateTime, opt => opt.Ignore());

        }


    }

}
