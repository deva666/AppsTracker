using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Task_Logger_Pro.Hooks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Task_Logger_Pro.Logging
{
    public class IdleMonitor : IDisposable
    {
        private bool _disposed = false;
        private bool _idleEntered = false;
        private bool _hooksRemoved = true;
        private bool _enabled = true;

        private Timer _idleTimer;

        KeyBoardHook.HookHandlerCallBack keyboardCallback = null;
        MouseHook.HookHandlerCallBack mouseHookCallback = null;

        IntPtr keyboardHookHandle = IntPtr.Zero;
        IntPtr mouseHookHandle = IntPtr.Zero;

        public event EventHandler IdleEntered;
        public event EventHandler IdleStoped;

        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
            }
        }

        public IdleMonitor()
        {
            _idleTimer = new Timer(CheckIdleState, null, 2 * 60 * 1000, 1000);
        }

        private void SetHooks()
        {
            if (keyboardHookHandle == IntPtr.Zero && mouseHookHandle == IntPtr.Zero)
            {
                keyboardCallback = new KeyBoardHook.HookHandlerCallBack(KeyboardHookProc);
                mouseHookCallback = new MouseHook.HookHandlerCallBack(MouseHookProc);
                using (Process process = Process.GetCurrentProcess())
                {
                    using (ProcessModule module = process.MainModule)
                    {
                        keyboardHookHandle = KeyBoardHook.Win32.SetWindowsHookEx(13, keyboardCallback, WinAPI.GetModuleHandle(module.ModuleName), 0);
                        mouseHookHandle = MouseHook.Win32.SetWindowsHookEx(14, mouseHookCallback, WinAPI.GetModuleHandle(module.ModuleName), 0);
                        Debug.Assert(keyboardHookHandle != IntPtr.Zero && mouseHookHandle != IntPtr.Zero, "Setting hooks failed");
                    }
                }
                _hooksRemoved = false;
            }
        }


        private IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
                ResetBase();
            return WinAPI.CallNextHookEx(keyboardHookHandle, nCode, wParam, lParam);
        }


        private IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
                ResetBase();
            return WinAPI.CallNextHookEx(mouseHookHandle, nCode, wParam, lParam);
        }

        private void ResetBase()
        {
            if (!_idleEntered)
                return;
            _idleEntered = false;
            RemoveHooks();
            _idleTimer.Change(2 * 60 * 1000, 1000);
            var handler = IdleStoped;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void CheckIdleState(object sender)
        {
            if (_idleEntered || !_enabled)
                return;
            IdleTimeInfo idleInfo = IdleTimeWatcher.GetIdleTimeInfo();
            if (idleInfo.IdleTime >= TimeSpan.FromMilliseconds(App.UzerSetting.IdleInterval))
            {
                _idleTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                _idleEntered = true;
                App.Current.Dispatcher.Invoke(SetHooks);
                var handler = IdleEntered;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        private void RemoveHooks()
        {
            WinAPI.UnhookWindowsHookEx(keyboardHookHandle);
            WinAPI.UnhookWindowsHookEx(mouseHookHandle);
            keyboardHookHandle = IntPtr.Zero;
            mouseHookHandle = IntPtr.Zero;
            keyboardCallback = null;
            mouseHookCallback = null;
            _hooksRemoved = true;
        }

        ~IdleMonitor()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;

                _idleTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                _idleTimer.Dispose();

                Delegate[] delegateBuffer = null;
                if (IdleEntered != null)
                {
                    delegateBuffer = IdleEntered.GetInvocationList();
                    foreach (EventHandler del in delegateBuffer)
                    {
                        IdleEntered -= del;
                    }
                    IdleEntered = null;
                }

                if (IdleStoped != null)
                {
                    delegateBuffer = IdleStoped.GetInvocationList();
                    foreach (EventHandler del in delegateBuffer)
                    {
                        IdleStoped -= del;
                    }
                    IdleStoped = null;
                }

                if (!_hooksRemoved)
                    RemoveHooks();
            }
        }
    }
}
