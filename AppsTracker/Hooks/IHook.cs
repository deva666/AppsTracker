using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Hooks
{
    public interface IHook<T> : IDisposable where T : EventArgs
    {
        event EventHandler<T> HookProc;
        void EnableHook(bool enable);
    }
}
