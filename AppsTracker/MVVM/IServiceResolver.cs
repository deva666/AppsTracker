using System;

namespace AppsTracker.MVVM
{
    public interface IServiceResolver
    {
        void Initialize(System.ComponentModel.Composition.Hosting.CompositionContainer container);

        T Resolve<T>() where T : AppsTracker.Data.Service.IBaseService;

        T Resolve<T>(string contract) where T : AppsTracker.Data.Service.IBaseService;
    }
}
