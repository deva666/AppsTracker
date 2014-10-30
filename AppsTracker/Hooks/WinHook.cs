using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using AppsTracker.Models.Proxy;


namespace AppsTracker.Hooks
{
    internal delegate void WinHookCallBack(IntPtr hWinEventHook, uint eventType, IntPtr hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    public sealed class WinHook : IHook<WinHookArgs>
    {
        #region Fields

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        private bool _isDisposed;
        private bool _isHookEnabled = true;

        public event EventHandler<WinHookArgs> HookProc;

        WinHookCallBack _winHookCallBack;

        IntPtr _hookID = IntPtr.Zero;

        #endregion

        #region Constructor

        public WinHook()
        {
            if (App.Current.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
                App.Current.Dispatcher.Invoke(SetHook);
            else
                SetHook();
        }

        private void SetHook()
        {
            _winHookCallBack = new WinHookCallBack(WinHookCallback);
            _hookID = WinAPI.SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _winHookCallBack, 0, 0, WINEVENT_OUTOFCONTEXT);
            Debug.Assert(_hookID != IntPtr.Zero, "Failed to set WinHook");
        }


        #endregion

        #region ClassMethods

        //public WinHookArgs GetWinEventArgs()
        //{
        //    IntPtr hWnd = WinAPI.GetForegroundWindow();
        //    return new WinHookArgs(GetActiveWindowName(), GetProcess(hWnd));
        //}

        //public string GetActiveWindowName()
        //{
        //    IntPtr foregroundWindow = WinAPI.GetForegroundWindow();
        //    StringBuilder windowTitle = new StringBuilder(WinAPI.GetWindowTextLength(foregroundWindow) + 1);
        //    if (WinAPI.GetWindowText(foregroundWindow, windowTitle, windowTitle.Capacity) > 0)
        //    {
        //        if (string.IsNullOrEmpty(windowTitle.ToString().Trim()))
        //            return "No Title";
        //        return windowTitle.ToString();
        //    }
        //    return "No Title";
        //}

        private Process GetProcess(IntPtr hWnd)
        {
            uint processID = 0;
            if (hWnd != IntPtr.Zero)
            {
                WinAPI.GetWindowThreadProcessId(hWnd, out processID);
                if (processID != 0)
                    return System.Diagnostics.Process.GetProcessById(Convert.ToInt32(processID));

            }
            return null;
        }

        #endregion

        #region CallBackMethod

        private void WinHookCallback(IntPtr hWinEventHook, uint eventType, IntPtr hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (!_isHookEnabled)
                return;

            if (hWnd != IntPtr.Zero)
            {
                StringBuilder windowTitleBuilder = new StringBuilder(WinAPI.GetWindowTextLength(hWnd) + 1);
                WinAPI.GetWindowText(hWnd, windowTitleBuilder, windowTitleBuilder.Capacity);
                var process = GetProcess(hWnd);
                var title = string.IsNullOrEmpty(windowTitleBuilder.ToString()) ? "No Title" : windowTitleBuilder.ToString();
                IAppInfo appInfo = AppInfo.GetAppInfo(process);
                HookProc.InvokeSafely<WinHookArgs>(this, new WinHookArgs(title, appInfo));
            }
        }

        #endregion

        #region IDisposable Members
        ~WinHook()
        {
            Dispose(false);
            Debug.WriteLine("WinEvent Finalizer called");

        }

        private void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.WriteLine("Disposing " + this.GetType().Name + " " + this.GetType().FullName);

            if (_isDisposed) return;
            WinAPI.UnhookWinEvent(_hookID);
            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public void EnableHook(bool enable)
        {
            _isHookEnabled = enable;
        }
    }
}

