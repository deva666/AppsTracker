#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.Text;
using System.Threading;
using AppsTracker.Common.Utils;
using AppsTracker.Communication;
using AppsTracker.Data.Utils;

namespace AppsTracker.Tracking.Hooks
{
    [Export(typeof(IAppChangedNotifier))]
    public sealed class AppChangedNotifier : IAppChangedNotifier
    {
        public event EventHandler<AppChangedArgs> AppChanged;

        private Boolean isDisposed = false;

        private readonly IWindowChangedNotifier windowChangedHook;
        private readonly ITitleChangedNotifier titleChangedHook;
        private readonly ISyncContext syncContext;

        private readonly Timer windowCheckTimer;

        private IntPtr activeWindowHandle;
        private String activeWindowTitle;


        [ImportingConstructor]
        public AppChangedNotifier(IWindowChangedNotifier windowChangedHook,
                                  ITitleChangedNotifier titleChangedHook,
                                  ISyncContext syncContext)
        {
            this.windowChangedHook = windowChangedHook;
            this.titleChangedHook = titleChangedHook;
            this.syncContext = syncContext;

            windowCheckTimer = new Timer(OnTimerTick, null, 5 * 1000, 5 * 1000);

            this.windowChangedHook.ActiveWindowChanged += OnActiveWindowChanged;
            this.titleChangedHook.TitleChanged += OnTitleChanged;
        }

        private void OnTimerTick(Object state)
        {
            syncContext.Invoke(() =>
            {
                var hWnd = NativeMethods.GetForegroundWindow();
                if(hWnd != IntPtr.Zero && hWnd != activeWindowHandle)
                {
                    SetActiveWindow(hWnd);
                }
            });
        }

        private void OnActiveWindowChanged(object sender, WinChangedArgs e)
        {
            if (activeWindowHandle == e.Handle ||
                activeWindowTitle == e.Title)
                return;

            activeWindowHandle = e.Handle;
            activeWindowTitle = e.Title;

            RaiseAppChanged();
        }

        private void OnTitleChanged(object sender, WinChangedArgs e)
        {
            if (activeWindowHandle != e.Handle)
                return;

            if (activeWindowTitle == e.Title)
                return;

            activeWindowTitle = e.Title;
            RaiseAppChanged();
        }

        public void CheckActiveApp()
        {
            var hWnd = NativeMethods.GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
                return;

            SetActiveWindow(hWnd);
        }

        private void SetActiveWindow(IntPtr hWnd)
        {
            activeWindowHandle = hWnd;
            var windowTitleBuilder = new StringBuilder();
            windowTitleBuilder.Capacity = NativeMethods.GetWindowTextLength(hWnd) + 1;
            NativeMethods.GetWindowText(hWnd, windowTitleBuilder, windowTitleBuilder.Capacity);
            var title = string.IsNullOrEmpty(windowTitleBuilder.ToString()) ?
                "No Title" : windowTitleBuilder.ToString();
            activeWindowTitle = title;

            RaiseAppChanged();
        }

        private void RaiseAppChanged()
        {
            var appInfo = AppInfo.Create(activeWindowHandle);
            var logInfo = LogInfo.Create(appInfo, activeWindowTitle);
            AppChanged.InvokeSafely(this, new AppChangedArgs(logInfo));
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                windowCheckTimer.Dispose();
                windowChangedHook.Dispose();
                titleChangedHook.Dispose();

                isDisposed = true;
            }
        }
    }
}

