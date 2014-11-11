using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Logging
{
    internal sealed class WindowHelper
    {
        public static string GetActiveWindowName()
        {
            IntPtr foregroundWindow = WinAPI.GetForegroundWindow();
            StringBuilder windowTitle = new StringBuilder(WinAPI.GetWindowTextLength(foregroundWindow) + 1);
            if (WinAPI.GetWindowText(foregroundWindow, windowTitle, windowTitle.Capacity) > 0)
            {
                if (string.IsNullOrEmpty(windowTitle.ToString().Trim())) return "No Title";
                return windowTitle.ToString();
            }
            return "No Title";
        }
    }
}
