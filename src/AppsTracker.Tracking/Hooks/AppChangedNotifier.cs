#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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

        private Boolean isDisposed = false;

        private readonly IWindowChangedNotifier windowChangedHook;
        private readonly ITitleChangedNotifier titleChangedHook;
        private readonly ISyncContext syncContext;
        private readonly IDisposable windowChangedSubscription;
        private readonly IDisposable titleChangedSubsription;
        private readonly Timer windowCheckTimer;
        private readonly Subject<AppChangedArgs> subject = new Subject<AppChangedArgs>();

        private IntPtr activeWindowHandle;
        private String activeWindowTitle;

        public IObservable<AppChangedArgs> AppChangedObservable
        {
            get
            {
                return subject.AsObservable();
            }
        }


        [ImportingConstructor]
        public AppChangedNotifier(IWindowChangedNotifier windowChangedHook,
                                  ITitleChangedNotifier titleChangedHook,
                                  ISyncContext syncContext)
        {
            this.windowChangedHook = windowChangedHook;
            this.titleChangedHook = titleChangedHook;
            this.syncContext = syncContext;
            this.windowCheckTimer = new Timer(OnTimerTick, null, 5 * 1000, 5 * 1000);

            windowChangedSubscription = CreateWindowChangedSubscription();
            titleChangedSubsription = CreateTitleChangedSubscription();
        }


        private IDisposable CreateWindowChangedSubscription()
        {
            return windowChangedHook.WinChangedObservable
                .Where(a => a.Title != activeWindowTitle && a.Handle != activeWindowHandle)
                .Do(a =>
                {
                    activeWindowHandle = a.Handle;
                    activeWindowTitle = a.Title;
                    NotifyAppChanged();
                })
                .Subscribe();
        }

        private IDisposable CreateTitleChangedSubscription()
        {
            return titleChangedHook.TitleChangedObservable
                .Where(a => a.Handle == activeWindowHandle && a.Title != activeWindowTitle)
                .Do(a => activeWindowTitle = a.Title)
                .Do(a => NotifyAppChanged())
                .Subscribe();
        }

        private void OnTimerTick(Object state)
        {
            syncContext.Invoke(() =>
            {
                var hWnd = NativeMethods.GetForegroundWindow();
                if (hWnd != IntPtr.Zero && hWnd != activeWindowHandle)
                {
                    SetActiveWindow(hWnd);
                }
            });
        }

        private void OnTitleChanged(object sender, WinChangedArgs e)
        {
            if (activeWindowHandle != e.Handle)
                return;

            if (activeWindowTitle == e.Title)
                return;

            activeWindowTitle = e.Title;
            NotifyAppChanged();
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

            NotifyAppChanged();
        }

        private void NotifyAppChanged()
        {
            var appInfo = AppInfo.Create(activeWindowHandle);
            var logInfo = LogInfo.Create(appInfo, activeWindowTitle);
            var args = new AppChangedArgs(logInfo);
            subject.OnNext(args);
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                windowCheckTimer.Dispose();
                windowChangedHook.Dispose();
                titleChangedHook.Dispose();
                windowChangedSubscription.Dispose();

                isDisposed = true;
            }
        }
    }
}

