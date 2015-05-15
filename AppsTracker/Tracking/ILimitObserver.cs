using System;
namespace AppsTracker.Tracking
{
    internal interface ILimitObserver : IDisposable
    {
        void Initialize();
    }
}
