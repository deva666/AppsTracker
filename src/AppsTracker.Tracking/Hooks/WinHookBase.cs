using System;
using AppsTracker.Common.Utils;

namespace AppsTracker.Tracking.Hooks
{
    public abstract class WinHookBase : IDisposable
    {
        private const uint WINEVENT_OUTOFCONTEXT = 0;

        private bool isDisposed;

        private readonly WinHookCallBack winHookCallBack;

        private readonly IntPtr hookID = IntPtr.Zero;

        public WinHookBase(uint minEvent, uint maxEvent)
        {
            winHookCallBack = new WinHookCallBack(WinHookCallback);
            hookID = WinAPI.SetWinEventHook(minEvent, maxEvent, IntPtr.Zero, winHookCallBack, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        protected abstract void WinHookCallback(IntPtr hWinEventHook, 
            uint eventType, IntPtr hWnd, int idObject, 
            int idChild, uint dwEventThread, uint dwmsEventTime);

        ~WinHookBase()
        {
            System.Diagnostics.Debug.WriteLine("WinHook finalizer called");
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (isDisposed)
                return;
            WinAPI.UnhookWinEvent(hookID);
            isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
