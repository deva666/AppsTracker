using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.ComponentModel;

namespace Task_Logger_Pro.Hooks
{
    public sealed class KeyBoardHook : HookBase, IDisposable, INotifyPropertyChanged
    {
        #region Fields

        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;
        const int WM_SYSKEYDOWN = 0x0104;
        const int WM_SYSKEYUP = 0x0105;
        const int VK_SHIFT = 0x10;
        const int VK_CONTROL = 0x11;
        const int VK_MENU = 0x12;
        const int VK_CAPITAL = 0x14;

        public event EventHandler<KeyboardHookArgs> KeyDown;
        public event EventHandler<KeyboardHookArgs> KeyPress;
        public event EventHandler<KeyboardHookArgs> KeyUp;

        HookHandlerCallBack hookCallBack;
        IntPtr hookID = IntPtr.Zero;

        bool keyLoggerEnabled = true;
        bool isDisposed;

        internal delegate IntPtr HookHandlerCallBack(int code, IntPtr wParam, IntPtr lParam);

        #endregion

        #region Properties

        public bool KeyLoggerEnabled
        {
            get
            {
                return keyLoggerEnabled;
            }
            set
            {
                keyLoggerEnabled = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("KeyLoggerEnabled"));
            }
        }

        #endregion

        #region Constructors

        public KeyBoardHook()
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId != App.Current.Dispatcher.Thread.ManagedThreadId)
                SetHookSameThread();
            else
                SetHook();
        }


        private void SetHookSameThread()
        {
            App.Current.Dispatcher.Invoke(SetHook);
        }

        protected override void SetHook()
        {
            hookCallBack = new HookHandlerCallBack(HookProc);
            using (Process process = Process.GetCurrentProcess())
            {
                using (ProcessModule module = process.MainModule)
                {
                    hookID = WinAPI.SetWindowsHookEx(WH_KEYBOARD_LL, hookCallBack, WinAPI.GetModuleHandle(module.ModuleName), 0);
                }
            }
        }

        #endregion

        #region HookCallBack

        private IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            if (!KeyLoggerEnabled || code < 0)
                return WinAPI.CallNextHookEx(hookID, code, wParam, lParam);

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
                        var handler = KeyDown;
                        if (handler != null)
                            handler(this, new KeyboardHookArgs(keybStruct.vkCode, builder.ToString()));
                    }
                }
            }

            if ((int)wParam == WM_KEYDOWN || (int)wParam == WM_SYSKEYDOWN)
            {
                var handler = KeyPress;
                if (handler != null)
                    handler(this, new KeyboardHookArgs(keybStruct.vkCode));
            }

            if ((int)wParam == WM_KEYUP || (int)wParam == WM_SYSKEYUP)
            {
                var handler = KeyUp;
                if (handler != null)
                    handler(this, new KeyboardHookArgs(keybStruct.vkCode));
            }

            return WinAPI.CallNextHookEx(hookID, code, wParam, lParam);
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

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

    }

}
