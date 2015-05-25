#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Text;
using AppsTracker.Data.Utils;
using AppsTracker.Common.Utils;

namespace AppsTracker.Tracking
{
    [Export(typeof(IWindowHelper))]
    internal sealed class WindowHelper : IWindowHelper
    {
        public string GetActiveWindowName()
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

        public IntPtr GetActiveWindowHandle()
        {
            return WinAPI.GetForegroundWindow();
        }

        public IAppInfo GetActiveWindowAppInfo()
        {
            var handle = GetActiveWindowHandle();
            if (handle == IntPtr.Zero)
                return null;

            var process = GetProcessFromHandle(handle);
            return AppInfo.GetAppInfo(process);
        }

        private Process GetProcessFromHandle(IntPtr hWnd)
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
