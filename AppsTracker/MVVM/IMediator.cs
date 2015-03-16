using System;

namespace AppsTracker.MVVM
{
    public interface IMediator
    {
        void Register<T>(string message, Action<T> callBack);
        void Register(string message, Action callBack);
        void NotifyColleagues<T>(string message, T parameter);
        void NotifyColleagues(string message);
    }
}
