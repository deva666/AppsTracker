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
using AppsTracker.Common.Utils;
using AppsTracker.Data.Utils;

namespace AppsTracker.Tracking.Hooks
{
    [Export(typeof(IAppChangedNotifier))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class AppChangedNotifier : IAppChangedNotifier
    {
        public event EventHandler<AppChangedArgs> AppChanged;

        private readonly IWinChanged winChangedHook;
        private readonly ITitleChanged titleChangedHook;

        private IntPtr activeWindowHandle;
        private String activeWindowTitle;


        [ImportingConstructor]
        public AppChangedNotifier(IWinChanged winChangedHook, ITitleChanged titleChangedHook)
        {
            this.winChangedHook = winChangedHook;
            this.titleChangedHook = titleChangedHook;

            this.winChangedHook.ActiveWindowChanged += OnActiveWindowChanged;
            this.titleChangedHook.TitleChanged += OnTitleChanged;
        }


        private void OnActiveWindowChanged(object sender, WinChangedArgs e)
        {
            if (activeWindowHandle == e.Handle)
                return;

            if (activeWindowTitle == e.Title)
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
            var hWnd = WinAPI.GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
                return;

            var windowTitleBuilder = new StringBuilder();
            windowTitleBuilder.Clear();
            windowTitleBuilder.Capacity = WinAPI.GetWindowTextLength(hWnd) + 1;
            WinAPI.GetWindowText(hWnd, windowTitleBuilder, windowTitleBuilder.Capacity);
            var title = string.IsNullOrEmpty(windowTitleBuilder.ToString()) ?
                "No Title" : windowTitleBuilder.ToString();
            activeWindowTitle = title;
            var appInfo = AppInfo.Create(hWnd);
            var logInfo = LogInfo.Create(appInfo, title);
            AppChanged.InvokeSafely(this, new AppChangedArgs(logInfo));
        }

        private void RaiseAppChanged()
        {
            var appInfo = AppInfo.Create(activeWindowHandle);
            var logInfo = LogInfo.Create(appInfo, activeWindowTitle);
            AppChanged.InvokeSafely(this, new AppChangedArgs(logInfo));
        }

        public void Dispose()
        {
            winChangedHook.Dispose();
            titleChangedHook.Dispose();
        }
    }
}

