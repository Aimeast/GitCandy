using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Reflection;
using System.Web.Mvc;

namespace GitCandy.Base
{
    public class MefDependencyResolver : IDependencyResolver
    {
        private readonly CompositionHost _container;
        private readonly IDependencyResolver _resolver;

        public MefDependencyResolver(AttributedModelProvider builder, IDependencyResolver resolver)
        {
            _container = new ContainerConfiguration()
                .WithAssembly(Assembly.GetExecutingAssembly(), builder)
                .CreateContainer();

            _resolver = resolver;
        }

        public object GetService(Type serviceType)
        {
            try
            {
                return _container.GetExport(serviceType);
            }
            catch
            {
                return _resolver.GetService(serviceType);
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return _container.GetExports(serviceType);
            }
            catch
            {
                return _resolver.GetServices(serviceType);
            }
        }
    }
}