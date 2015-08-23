#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AppsTracker.Common.Utils;

namespace AppsTracker.Tracking.Hooks
{
    internal sealed class MouseHook : IDisposable
    {
        private bool isDisposed;
        private bool isHookEnabled = true;

        private const int WH_MOUSE_LL = 14;

        public event EventHandler<EventArgs> HookProc;

        private WinAPI.MSLLHOOKSTRUCT mouseStruct;

        private readonly MouseHookCallback hookCallBack;

        private IntPtr hookID = IntPtr.Zero;

        public MouseHook()
        {
            hookCallBack = new MouseHookCallback(MouseHookCallback);
            using (var process = Process.GetCurrentProcess())
            {
                using (var module = process.MainModule)
                {
                    hookID = WinAPI.SetWindowsHookEx(WH_MOUSE_LL, hookCallBack, WinAPI.GetModuleHandle(module.ModuleName), 0);
                    Debug.Assert(hookID != IntPtr.Zero, "Failed to set MouseHooke");
                }
            }
        }

        private IntPtr MouseHookCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0 || !isHookEnabled)
                return WinAPI.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);

            mouseStruct = (WinAPI.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(WinAPI.MSLLHOOKSTRUCT));

            HookProc.InvokeSafely(this, new EventArgs());

            return WinAPI.CallNextHookEx(hookID, code, wParam, lParam);
        }

        ~MouseHook()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (isDisposed)
                return;
            Debug.WriteLine("Disposing " + this.GetType().Name + " " + this.GetType().FullName);
            WinAPI.UnhookWindowsHookEx(hookID);
            isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
