using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Common.Utils;
using AppsTracker.Data.Utils;

namespace AppsTracker.Tracking.Hooks
{
    internal sealed class WinChangedHook : WinHookBase
    {
        public event EventHandler<WinChangedArgs> ActiveWindowChanged;

        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        private readonly StringBuilder windowTitleBuilder = new StringBuilder();

        public WinChangedHook()
            : base(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND)
        {

        }

        protected override void WinHookCallback(IntPtr hWinEventHook, uint eventType, IntPtr hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hWnd == IntPtr.Zero)
                return;

            windowTitleBuilder.Clear();
            windowTitleBuilder.Capacity = WinAPI.GetWindowTextLength(hWnd) + 1;
            WinAPI.GetWindowText(hWnd, windowTitleBuilder, windowTitleBuilder.Capacity);
            var title = string.IsNullOrEmpty(windowTitleBuilder.ToString()) ?
                "No Title" : windowTitleBuilder.ToString();
            ActiveWindowChanged.InvokeSafely(this, new WinChangedArgs(title, hWnd));
        }
    }
}
