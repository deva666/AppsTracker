#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using AppsTracker.Data.Utils;
using System;
using System.Diagnostics;
using System.Text;

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

        public static IntPtr GetActiveWindowHandle()
        {
            return WinAPI.GetForegroundWindow();
        }

        public static IAppInfo GetActiveWindowAppInfo()
        {
            var handle = GetActiveWindowHandle();
            if (handle == IntPtr.Zero)
                return null;

            var process = GetProcessFromHandle(handle);
            return AppInfo.GetAppInfo(process);
        }

        private static Process GetProcessFromHandle(IntPtr hWnd)
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
    }
}
