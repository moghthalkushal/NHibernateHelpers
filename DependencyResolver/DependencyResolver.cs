using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Dependencies;
using Microsoft.Practices.ServiceLocation;
using StructureMap;

namespace NHibernateHelper.Dependany.Resolver
{
 public   class StructureMapDependencyResolver : ServiceLocatorImplBase, IDependencyResolver
    {
        public StructureMapDependencyResolver(IContainer container)
        {
            _container = container;
        }

        private readonly IContainer _container;

        public void Dispose()
        {
            _container.Dispose();
        }

        public IDependencyScope BeginScope()
        {
            return new StructureMapDependencyResolver(_container.GetNestedContainer());
        }

        public new object GetService(Type serviceType)
        {
            if (serviceType == null) return null;

            try
            {
                return serviceType.IsAbstract || serviceType.IsInterface
                           ? _container.TryGetInstance(serviceType)
                           : _container.GetInstance(serviceType);
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _container.GetAllInstances(serviceType).Cast<object>();
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            return _container.GetAllInstances(serviceType).Cast<object>();
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            return !String.IsNullOrEmpty(key) ? _container.GetInstance(serviceType, key) : GetService(serviceType);
        }
    }
}