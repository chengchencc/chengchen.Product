using System.Linq;
using System.Web.Mvc;
using BlackMamba.Framework.Core;
using StructureMap;
using BlackMamba.Billing.Domain.Mappers;
using BlackMamba.Billing.Domain;

namespace BlackMamba.Billing.Domain
{
    public class Bootstrapper
    {
        public static bool _hasBeenIntialized = false;

        public static void Start()
        {
            if (!_hasBeenIntialized)
            {
                EntityMapping.Config();

                ConfigueInjection();

                _hasBeenIntialized = true;
            }
        }


        public static void ConfigueInjection()
        {
            DependencyResolver.SetResolver(
                t =>
                {
                    try
                    {
                        return ObjectFactory.GetInstance(t);
                    }
                    catch
                    {
                        return null;
                    }
                   
                },
                t => ObjectFactory.GetAllInstances<object>().Where(s => s.GetType() == t));


            //if (ConfigKeys.ENABLE_SNAP.ConfigValue().ToBoolean())
            //{
            //    SnapConfiguration.For<StructureMapAspectContainer>(c =>
            //    {
            //        c.IncludeNamespace("Youle.Mobile.Infrastructure.Domain.Services*");
            //        c.Bind<ServiceCacheInterceptor>().To<ServiceCacheAttribute>();
            //    });
            //}

            ObjectFactory.Configure(x => x.AddRegistry(new ControllerRegistry())); 
        }
    }
}