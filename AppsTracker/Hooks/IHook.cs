#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;

namespace AppsTracker.Hooks
{
    public interface IHook<T> : IDisposable where T : EventArgs
    {
        event EventHandler<T> HookProc;
        void EnableHook(bool enable);
    }
}
