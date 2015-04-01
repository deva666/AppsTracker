using System;

namespace AppsTracker.ServiceLocation
{
    public interface IServiceResolver
    {
        void Initialize(System.ComponentModel.Composition.Hosting.CompositionContainer container);

        T Resolve<T>() where T : AppsTracker.Service.IBaseService;

        T Resolve<T>(string contract) where T : AppsTracker.Service.IBaseService;
    }
}
