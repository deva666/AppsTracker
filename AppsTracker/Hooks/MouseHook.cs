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

namespace AppsTracker.Hooks
{
    public enum MouseButton
    {
        LeftDown,
        RightDown,
        LeftUp,
        RightUp,
        None
    }

    internal delegate IntPtr MouseHookCallback(int code, IntPtr wParam, IntPtr lParam);

    internal sealed class MouseHook : IDisposable
    {
        private bool isDisposed;
        private bool isHookEnabled = true;

        internal const int WM_LBUTTONDOWN = 0x0201;
        internal const int WM_RBUTTONDOWN = 0x0204;
        internal const int WM_LBUTTONUP = 0x0202;
        internal const int WM_RBUTTONUP = 0x0205;

        private const int WH_MOUSE_LL = 14;

        public event EventHandler<MouseHookArgs> HookProc;

        private WinAPI.MSLLHOOKSTRUCT mouseStruct;

        private MouseHookCallback hookCallBack;

        private IntPtr hookID = IntPtr.Zero;

        public MouseHook()
        {
            SetHook();
        }

        IntPtr MouseHookCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0 || !isHookEnabled)
                return WinAPI.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);

            mouseStruct = (WinAPI.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(WinAPI.MSLLHOOKSTRUCT));

            HookProc.InvokeSafely(this, new MouseHookArgs(mouseStruct.pt, (int)wParam));

            return WinAPI.CallNextHookEx(hookID, code, wParam, lParam);
        }

        ~MouseHook()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.WriteLine("Disposing " + this.GetType().Name + " " + this.GetType().FullName);
            if (isDisposed) return;
            WinAPI.UnhookWindowsHookEx(hookID);
            isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void SetHook()
        {
            hookCallBack = new MouseHookCallback(MouseHookCallback);
            using (Process process = Process.GetCurrentProcess())
            {
                using (ProcessModule module = process.MainModule)
                {
                    hookID = WinAPI.SetWindowsHookEx(WH_MOUSE_LL, hookCallBack, WinAPI.GetModuleHandle(module.ModuleName), 0);
                    Debug.Assert(hookID != IntPtr.Zero, "Failed to set MouseHooke");
                }
            }
        }

        public void EnableHook(bool enable)
        {
            isHookEnabled = enable;
        }
    }
}
