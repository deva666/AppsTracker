using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;

using AppsTracker.Hooks;

namespace AppsTracker.Logging
{
    public class IdleMonitor : IDisposable
    {
        private bool _disposed = false;
        private bool _idleEntered = false;
        private bool _hooksRemoved = true;
        private bool _enabled = true;

        private Timer _idleTimer;

        KeyboardHookCallback _keyboardCallback = null;
        MouseHookCallback _mouseHookCallback = null;

        IntPtr _keyboardHookHandle = IntPtr.Zero;
        IntPtr _mouseHookHandle = IntPtr.Zero;

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
            _idleTimer = new Timer(CheckIdleState, null, 1 * 60 * 1000, 1000);
        }

        private void SetHooks()
        {
            if (_keyboardHookHandle == IntPtr.Zero && _mouseHookHandle == IntPtr.Zero)
            {
                _keyboardCallback = new KeyboardHookCallback(KeyboardHookCallback);
                _mouseHookCallback = new MouseHookCallback(MouseHookCallback);
                using (Process process = Process.GetCurrentProcess())
                {
                    using (ProcessModule module = process.MainModule)
                    {
                        _keyboardHookHandle = WinAPI.SetWindowsHookEx(13, _keyboardCallback, WinAPI.GetModuleHandle(module.ModuleName), 0);
                        _mouseHookHandle = WinAPI.SetWindowsHookEx(14, _mouseHookCallback, WinAPI.GetModuleHandle(module.ModuleName), 0);
                        Debug.Assert(_keyboardHookHandle != IntPtr.Zero && _mouseHookHandle != IntPtr.Zero, "Setting hooks failed");
                    }
                }
                _hooksRemoved = false;
            }
        }


        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
                ResetBase();
            return WinAPI.CallNextHookEx(_keyboardHookHandle, nCode, wParam, lParam);
        }


        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
                ResetBase();
            return WinAPI.CallNextHookEx(_mouseHookHandle, nCode, wParam, lParam);
        }

        private void ResetBase()
        {
            if (!_idleEntered)
                return;
            _idleEntered = false;
            RemoveHooks();
            _idleTimer.Change(1 * 60 * 1000, 1000);
            IdleStoped.InvokeSafely(this, EventArgs.Empty);
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
                IdleEntered.InvokeSafely(this, EventArgs.Empty);
            }
        }

        private void RemoveHooks()
        {
            WinAPI.UnhookWindowsHookEx(_keyboardHookHandle);
            WinAPI.UnhookWindowsHookEx(_mouseHookHandle);

            _keyboardHookHandle = IntPtr.Zero;
            _mouseHookHandle = IntPtr.Zero;
            _keyboardCallback = null;
            _mouseHookCallback = null;
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
