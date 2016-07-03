using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using AppsTracker.Common.Utils;

namespace AppsTracker.Tracking.Hooks
{
    [Export(typeof(ITitleChangedNotifier))]
    public sealed class TitleChangedHook : HookBase, ITitleChangedNotifier
    {
        private readonly StringBuilder windowTitleBuilder = new StringBuilder();
        private readonly Subject<WinChangedArgs> subject = new Subject<WinChangedArgs>();

        private const uint EVENT_OBJECT_NAMECHANGE = 0x800C;

        public IObservable<WinChangedArgs> TitleChangedObservable
        {
            get { return subject.AsObservable(); }
        }


        public TitleChangedHook()
            : base(EVENT_OBJECT_NAMECHANGE, EVENT_OBJECT_NAMECHANGE)
        {
        }


        protected override void WinHookCallback(IntPtr hWinEventHook,
            uint eventType, IntPtr hWnd, int idObject,
            int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hWnd == IntPtr.Zero || idChild < 0 || idObject != 0)
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
