#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Diagnostics;
using System.Text;
using AppsTracker.Data.Utils;


namespace AppsTracker.Hooks
{
    internal delegate void WinHookCallBack(IntPtr hWinEventHook, uint eventType, IntPtr hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    public sealed class WinHook : IWindowNotifier
    {
        public event EventHandler<WindowChangedArgs> WindowChanged;

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        private bool isDisposed;

        private WinHookCallBack winHookCallBack;

        private IntPtr hookID = IntPtr.Zero;

        public WinHook()
        {
            if (App.Current.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
                App.Current.Dispatcher.Invoke(SetHook);
            else
                SetHook();
        }

        private void SetHook()
        {
            winHookCallBack = new WinHookCallBack(WinHookCallback);
            hookID = WinAPI.SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, winHookCallBack, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        private Process GetProcessFromHandle(IntPtr hWnd)
        {
            uint processID = 0;
            if (hWnd != IntPtr.Zero)
            {
                WinAPI.GetWindowThreadProcessId(hWnd, out processID);
                if (processID != 0)
                    return System.Diagnostics.Process.GetProcessById(checked((int)processID));

            }
            return null;
        }

        private void WinHookCallback(IntPtr hWinEventHook, uint eventType, IntPtr hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hWnd == IntPtr.Zero)
                return;

            StringBuilder windowTitleBuilder = new StringBuilder(WinAPI.GetWindowTextLength(hWnd) + 1);
            WinAPI.GetWindowText(hWnd, windowTitleBuilder, windowTitleBuilder.Capacity);
            var process = GetProcessFromHandle(hWnd);
            var title = string.IsNullOrEmpty(windowTitleBuilder.ToString()) ? "No Title" : windowTitleBuilder.ToString();
            IAppInfo appInfo = AppInfo.GetAppInfo(process);
            WindowChanged.InvokeSafely(this, new WindowChangedArgs(title, appInfo));
        }

        ~WinHook()
        {
            Dispose(false);
            Debug.WriteLine("WinEvent Finalizer called");
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

