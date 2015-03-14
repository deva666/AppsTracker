using System;
using System.Diagnostics;
using System.Threading;

using AppsTracker.Data.Service;
using AppsTracker.Hooks;

namespace AppsTracker.Logging
{
    public class IdleMonitor : IDisposable
    {
        private readonly ISqlSettingsService settingsService;

        private bool disposed = false;
        private bool idleEntered = false;
        private bool hooksRemoved = true;

        private Timer idleTimer;

        private KeyboardHookCallback keyboardHookCallback = null;
        private MouseHookCallback mouseHookCallback = null;

        private IntPtr keyboardHookHandle = IntPtr.Zero;
        private IntPtr mouseHookHandle = IntPtr.Zero;

        public event EventHandler IdleEntered;
        public event EventHandler IdleStoped;

        public IdleMonitor()
        {
            settingsService = ServiceFactory.Get<ISqlSettingsService>();
            idleTimer = new Timer(CheckIdleState, null, 1 * 60 * 1000, 1000);
        }

        private void SetHooks()
        {
            if (keyboardHookHandle == IntPtr.Zero && mouseHookHandle == IntPtr.Zero)
            {
                keyboardHookCallback = new KeyboardHookCallback(KeyboardHookCallback);
                mouseHookCallback = new MouseHookCallback(MouseHookCallback);
                using (Process process = Process.GetCurrentProcess())
                {
                    using (ProcessModule module = process.MainModule)
                    {
                        keyboardHookHandle = WinAPI.SetWindowsHookEx(13, keyboardHookCallback, WinAPI.GetModuleHandle(module.ModuleName), 0);
                        mouseHookHandle = WinAPI.SetWindowsHookEx(14, mouseHookCallback, WinAPI.GetModuleHandle(module.ModuleName), 0);
                        Debug.Assert(keyboardHookHandle != IntPtr.Zero && mouseHookHandle != IntPtr.Zero, "Setting hooks failed");
                    }
                }
                hooksRemoved = false;
            }
        }


        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
                ResetBase();
            return WinAPI.CallNextHookEx(keyboardHookHandle, nCode, wParam, lParam);
        }


        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
                ResetBase();
            return WinAPI.CallNextHookEx(mouseHookHandle, nCode, wParam, lParam);
        }

        private void ResetBase()
        {
            if (!idleEntered)
                return;
            idleEntered = false;
            RemoveHooks();
            idleTimer.Change(1 * 60 * 1000, 1000);
            IdleStoped.InvokeSafely(this, EventArgs.Empty);
        }

        private void CheckIdleState(object sender)
        {
            if (idleEntered)
                return;

            IdleTimeInfo idleInfo = IdleTimeWatcher.GetIdleTimeInfo();

            if (idleInfo.IdleTime >= TimeSpan.FromMilliseconds(settingsService.Settings.IdleTimer))
            {
                idleTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                idleEntered = true;
                App.Current.Dispatcher.Invoke(SetHooks);
                IdleEntered.InvokeSafely(this, EventArgs.Empty);
            }
        }

        private void RemoveHooks()
        {
            WinAPI.UnhookWindowsHookEx(keyboardHookHandle);
            WinAPI.UnhookWindowsHookEx(mouseHookHandle);

            keyboardHookHandle = IntPtr.Zero;
            mouseHookHandle = IntPtr.Zero;
            keyboardHookCallback = null;
            mouseHookCallback = null;
            hooksRemoved = true;
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
            if (!disposed)
            {
                disposed = true;

                idleTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                idleTimer.Dispose();

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

                if (!hooksRemoved)
                    RemoveHooks();
            }
        }
    }
}
