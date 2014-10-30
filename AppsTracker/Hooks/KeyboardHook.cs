using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.ComponentModel;

namespace AppsTracker.Hooks
{
    internal delegate IntPtr KeyboardHookCallback(int code, IntPtr wParam, IntPtr lParam);

    internal sealed class KeyBoardHook : IHook<KeyboardHookArgs>, INotifyPropertyChanged
    {
        #region Fields

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;
        private const int VK_SHIFT = 0x10;
        private const int VK_CONTROL = 0x11;
        private const int VK_MENU = 0x12;
        private const int VK_CAPITAL = 0x14;

        public event EventHandler<KeyboardHookArgs> HookProc;

        KeyboardHookCallback _hookCallBack;
        IntPtr _hookID = IntPtr.Zero;

        private bool _keyLoggerEnabled = true;
        private bool _isDisposed;

        #endregion

        #region Properties

        public bool KeyLoggerEnabled
        {
            get
            {
                return _keyLoggerEnabled;
            }
            set
            {
                _keyLoggerEnabled = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("KeyLoggerEnabled"));
            }
        }

        #endregion

        #region Constructors

        public KeyBoardHook()
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId != App.Current.Dispatcher.Thread.ManagedThreadId)
                App.Current.Dispatcher.Invoke(SetHook);
            else
                SetHook();
        }

        private void SetHook()
        {
            _hookCallBack = new KeyboardHookCallback(HookCallback);
            using (Process process = Process.GetCurrentProcess())
            {
                using (ProcessModule module = process.MainModule)
                {
                    _hookID = WinAPI.SetWindowsHookEx(WH_KEYBOARD_LL, _hookCallBack, WinAPI.GetModuleHandle(module.ModuleName), 0);
                    Debug.Assert(_hookID != IntPtr.Zero, "Failed to set keyboardhook");
                }
            }
        }

        public void EnableHook(bool enable)
        {
            _keyLoggerEnabled = enable;
        }

        #endregion

        #region HookCallBack

        private IntPtr HookCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            if (!_keyLoggerEnabled || code < 0)
                return WinAPI.CallNextHookEx(_hookID, code, wParam, lParam);

            WinAPI.KBDLLHOOKSTRUCT keybStruct = (WinAPI.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(WinAPI.KBDLLHOOKSTRUCT));

            if ((int)wParam == WM_KEYDOWN || (int)wParam == WM_SYSKEYDOWN)
            {
                bool isDownShift = ((WinAPI.GetKeyState(VK_SHIFT) & 0x80) == 0x80 ? true : false);
                bool isDownCapslock = (WinAPI.GetKeyState(VK_CAPITAL) != 0 ? true : false);
                StringBuilder builder = new StringBuilder(10);
                byte[] lpKeyState = new byte[256];
                if (WinAPI.GetKeyboardState(lpKeyState))
                {
                    if (WinAPI.ToUnicodeEx((uint)keybStruct.vkCode
                                            , (uint)keybStruct.scanCode
                                            , lpKeyState, builder
                                            , builder.Capacity
                                            , 0
                                            , WinAPI.GetKeyboardLayout(0)) != -1)
                    {
                        HookProc.InvokeSafely<KeyboardHookArgs>(this, new KeyboardHookArgs(keybStruct.vkCode, builder.ToString()));
                    }
                }
            }
            return WinAPI.CallNextHookEx(_hookID, code, wParam, lParam);
        }

        #endregion

        #region IDisposable Members

        ~KeyBoardHook()
        {
            Dispose(false);
            Debug.WriteLine("KeyboardHook Finalizer called");
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

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

    }

}
