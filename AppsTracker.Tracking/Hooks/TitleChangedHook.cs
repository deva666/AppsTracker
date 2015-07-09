using System;
using System.ComponentModel.Composition;
using System.Text;
using AppsTracker.Common.Utils;

namespace AppsTracker.Tracking.Hooks
{
    [Export(typeof(ITitleChanged))]
    public sealed class TitleChangedHook : WinHookBase, ITitleChanged
    {
        public event EventHandler<WinChangedArgs> TitleChanged;

        private readonly StringBuilder windowTitleBuilder = new StringBuilder();

        private const uint EVENT_OBJECT_NAMECHANGE = 0x800C;

        public TitleChangedHook()
            : base(EVENT_OBJECT_NAMECHANGE, EVENT_OBJECT_NAMECHANGE)
        {

        }


        protected override void WinHookCallback(IntPtr hWinEventHook, uint eventType, IntPtr hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hWnd == IntPtr.Zero || idChild < 0 || idObject != 0)
                return;

            windowTitleBuilder.Clear();
            windowTitleBuilder.Capacity = WinAPI.GetWindowTextLength(hWnd) + 1;
            WinAPI.GetWindowText(hWnd, windowTitleBuilder, windowTitleBuilder.Capacity);
            var title = string.IsNullOrEmpty(windowTitleBuilder.ToString()) ?
                "No Title" : windowTitleBuilder.ToString();
            TitleChanged.InvokeSafely(this, new WinChangedArgs(title, hWnd));
        }
    }
}
