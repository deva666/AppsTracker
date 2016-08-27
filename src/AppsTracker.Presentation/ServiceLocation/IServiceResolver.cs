using System;

namespace AppsTracker.ServiceLocation
{
    public interface IServiceResolver
    {
        void Initialize(System.ComponentModel.Composition.Hosting.ExportProvider container);

        T Resolve<T>();

        T Resolve<T>(string contract);
    }
}
