#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
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

    internal sealed class MouseHook : IHook<MouseHookArgs>
    {
        #region Fields

        bool _isDisposed;
        bool _isHookEnabled = true;

        internal const int WM_LBUTTONDOWN = 0x0201;
        internal const int WM_RBUTTONDOWN = 0x0204;
        internal const int WM_LBUTTONUP = 0x0202;
        internal const int WM_RBUTTONUP = 0x0205;

        private const int WH_MOUSE_LL = 14;

        public event EventHandler<MouseHookArgs> HookProc;

        WinAPI.MSLLHOOKSTRUCT _mouseStruct;

        MouseHookCallback _hookCallBack;

        IntPtr _hookID = IntPtr.Zero;

        #endregion

        #region Constructor

        public MouseHook()
        {
            SetHook();
        }

        #endregion

        #region HookCallBack

        IntPtr MouseHookCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0 || !_isHookEnabled)
                return WinAPI.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);

            _mouseStruct = (WinAPI.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(WinAPI.MSLLHOOKSTRUCT));

            HookProc.InvokeSafely<MouseHookArgs>(this, new MouseHookArgs(_mouseStruct.pt, (int)wParam));

            return WinAPI.CallNextHookEx(_hookID, code, wParam, lParam);
        }

        #endregion

        #region IDisposable Members

        ~MouseHook()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.WriteLine("Disposing " + this.GetType().Name + " " + this.GetType().FullName);
            if (_isDisposed) return;
            WinAPI.UnhookWindowsHookEx(_hookID);
            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private void SetHook()
        {
            _hookCallBack = new MouseHookCallback(MouseHookCallback);
            using (Process process = Process.GetCurrentProcess())
            {
                using (ProcessModule module = process.MainModule)
                {
                    _hookID = WinAPI.SetWindowsHookEx(WH_MOUSE_LL, _hookCallBack, WinAPI.GetModuleHandle(module.ModuleName), 0);
                    Debug.Assert(_hookID != IntPtr.Zero, "Failed to set MouseHooke");
                }
            }
        }

        public void EnableHook(bool enable)
        {
            _isHookEnabled = enable;
        }
    }
}
