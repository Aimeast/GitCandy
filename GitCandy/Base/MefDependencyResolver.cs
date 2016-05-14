using GitCandy.Log;
using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace GitCandy.Base
{
    public class MefDependencyResolver : IDependencyResolver
    {
        private readonly CompositionHost _container;
        private readonly IDependencyResolver _resolver;

        public MefDependencyResolver(AttributedModelProvider builder, IDependencyResolver resolver)
        {
            var searchPath = HttpRuntime.BinDirectory;
            Logger.Info("Addions search path: {0}", searchPath);

            var addions = Directory.GetFiles(searchPath, "gitcandy.*.dll")
                .Select(x => Assembly.Load(File.ReadAllBytes(x)))
                .ToList();

            Logger.Info("It is found {0} DLLs for resloving addion: {1}",
                addions.Count,
                string.Concat(addions.Select(x => x.GetName().Name + " ").ToArray()));

            _container = new ContainerConfiguration()
                .WithAssembly(Assembly.GetCallingAssembly(), builder)
                .WithAssemblies(addions, builder)
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