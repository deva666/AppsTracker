using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Task_Logger_Pro.Hooks
{
    public enum MouseButton
    {
        LeftDown,
        RightDown,
        LeftUp,
        RightUp,
        None
    }
    class MouseHook : IDisposable
    {
        #region Fields

        bool isDisposed;

        internal const int WM_LBUTTONDOWN = 0x0201;
        internal const int WM_RBUTTONDOWN = 0x0204;
        internal const int WM_LBUTTONUP = 0x0202;
        internal const int WM_RBUTTONUP = 0x0205;
        const int WH_MOUSE_LL = 14;

        Point point;
        WinAPI.MSLLHOOKSTRUCT mouseStruct;

        public event EventHandler<MouseHookArgs> MouseMove;
        public event EventHandler<MouseHookArgs> MouseClickUp;
        public event EventHandler<MouseHookArgs> MouseClickDown;

        internal delegate IntPtr HookHandlerCallBack(int code, IntPtr wParam, IntPtr lParam);

        internal HookHandlerCallBack hookCallBack;
        IntPtr hookID = IntPtr.Zero;

        #endregion

        #region Constructor

        public MouseHook()
        {
            point = new Point();
            hookCallBack = new HookHandlerCallBack(HookProc);
            using (Process process = Process.GetCurrentProcess())
            {
                using (ProcessModule module = process.MainModule)
                {
                    hookID = Win32.SetWindowsHookEx(WH_MOUSE_LL, hookCallBack, WinAPI.GetModuleHandle(module.ModuleName), 0);
                }
            }
        }

        #endregion

        #region HookCallBack

        IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0)
            {
                return WinAPI.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
            }
            mouseStruct = (WinAPI.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(WinAPI.MSLLHOOKSTRUCT));
            if (point != mouseStruct.pt)
            {
                var handler = MouseMove;
                if (handler != null) handler(this, new MouseHookArgs(point, (int)wParam));
            }
            point = mouseStruct.pt;

            if (wParam == (IntPtr)WM_LBUTTONDOWN || wParam == (IntPtr)WM_RBUTTONDOWN)
            {
                var handler = MouseClickDown;
                if (handler != null) handler(this, new MouseHookArgs(mouseStruct.pt, (int)wParam));
            }

            if (wParam == (IntPtr)WM_LBUTTONUP || wParam == (IntPtr)WM_RBUTTONUP)
            {
                var handler = MouseClickUp;
                if (handler != null) handler(this, new MouseHookArgs(mouseStruct.pt, (int)wParam));
            }

            return WinAPI.CallNextHookEx(hookID, code, wParam, lParam);
        }

        #endregion

        internal class Win32
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr SetWindowsHookEx(int idHook, HookHandlerCallBack lpfn, IntPtr hMod, uint dwThreadId);
        }

        #region IDisposable Members

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

        #endregion
    }

    public class MouseHookArgs : EventArgs
    {
        Point _point;
        int _mouseButton;

        public Point Point { get { return _point; } }
        public MouseButton MouseButton
        {
            get
            {
                if (_mouseButton == MouseHook.WM_LBUTTONDOWN)
                {
                    return MouseButton.LeftDown;
                }
                else if (_mouseButton == MouseHook.WM_RBUTTONDOWN)
                {
                    return MouseButton.RightDown;
                }
                else if (_mouseButton == MouseHook.WM_LBUTTONUP)
                {
                    return MouseButton.LeftUp;
                }
                else if (_mouseButton == MouseHook.WM_RBUTTONUP)
                {
                    return MouseButton.RightUp;
                }
                else
                    return MouseButton.None;
            }
        }

        public MouseHookArgs(Point point, int mouseButton)
        {
            _point = point;
            _mouseButton = mouseButton;
        }

    }
}
