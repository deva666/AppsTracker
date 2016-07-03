using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using AppsTracker.Common.Utils;

namespace AppsTracker.Tracking.Hooks
{
    [Export(typeof(IWindowChangedNotifier))]
    internal sealed class WindowChangedHook : HookBase, IWindowChangedNotifier
    {
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        private readonly StringBuilder windowTitleBuilder = new StringBuilder();
        private readonly Subject<WinChangedArgs> subject = new Subject<WinChangedArgs>();

        public IObservable<WinChangedArgs> WinChangedObservable
        {
            get { return subject.AsObservable(); }
        }

        public WindowChangedHook()
            : base(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND)
        {
        }

        protected override void WinHookCallback(IntPtr hWinEventHook,
            uint eventType, IntPtr hWnd, int idObject,
            int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hWnd == IntPtr.Zero)
                return;

            windowTitleBuilder.Clear();
            windowTitleBuilder.Capacity = NativeMethods.GetWindowTextLength(hWnd) + 1;
            NativeMethods.GetWindowText(hWnd, windowTitleBuilder, windowTitleBuilder.Capacity);
            var title = string.IsNullOrEmpty(windowTitleBuilder.ToString()) ?
                "No Title" : windowTitleBuilder.ToString();
            var args = new WinChangedArgs(title, hWnd);
            subject.OnNext(args);
        }
    }
}
