using System;
using System.ComponentModel.Composition.Hosting;
using AppsTracker.Data.Service;

namespace AppsTracker.MVVM
{
    internal sealed class ServiceResolver : AppsTracker.MVVM.IServiceResolver
    {
        private static Lazy<ServiceResolver> instance = new Lazy<ServiceResolver>(() => new ServiceResolver());
        public static ServiceResolver Instance
        {
            get { return instance.Value; }
        }

        private CompositionContainer container;

        public void Initialize(CompositionContainer container)
        {
            this.container = container;
        }

        public T Resolve<T>() where T : IBaseService
        {
            if (container == null)
                throw new InvalidOperationException("Container not initialized");

            return container.GetExportedValue<T>();
        }

        public T Resolve<T>(string contract) where T : IBaseService
        {
            if (container == null)
                throw new InvalidOperationException("Container not initialized");

            return container.GetExportedValue<T>(contract);
        }
    }
}
