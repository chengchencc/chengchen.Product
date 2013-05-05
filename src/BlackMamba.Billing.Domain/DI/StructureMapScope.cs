using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Dependencies;

namespace BlackMamba.Billing.Domain
{
    public class StructureMapScope : IDependencyScope
    {
        public IContainer Container;

        public StructureMapScope(IContainer container)
        {
            Container = container;
        }

        public void Dispose()
        {
            // we will not dispose the container here
            // for it will not be constructed via ctor

            //Container = null;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                return null;
            }
            try
            {
                if (serviceType.IsAbstract || serviceType.IsInterface)
                    return Container.TryGetInstance(serviceType);

                return Container.GetInstance(serviceType);
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return Container.GetAllInstances<object>().Where(s => s.GetType() == serviceType);
        }
    }
}
