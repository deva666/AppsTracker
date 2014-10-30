using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.MVVM
{
    public interface IMediator
    {
        void Register(object target);
        void Register(string message, Delegate callback);
        void NotifyColleagues<T>(string message, T parameter);
        void NotifyColleagues<T>(string message);
    }
}
