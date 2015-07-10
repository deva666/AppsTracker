#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using AppsTracker.Common.Utils;

namespace AppsTracker.Tracking.Hooks
{
    internal sealed class KeyBoardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int VK_SHIFT = 0x10;
        private const int VK_CAPITAL = 0x14;

        private bool isDisposed;

        public event EventHandler<KeyboardHookArgs> HookProc;

        private readonly KeyboardHookCallback hookCallBack;
        private IntPtr hookID = IntPtr.Zero;

        private readonly KeycodeMap keycodeMap = new KeycodeMap();

        public KeyBoardHook()
        {
            hookCallBack = new KeyboardHookCallback(HookCallback);
            using (var process = Process.GetCurrentProcess())
            {
                using (var module = process.MainModule)
                {
                    hookID = WinAPI.SetWindowsHookEx(WH_KEYBOARD_LL, hookCallBack, WinAPI.GetModuleHandle(module.ModuleName), 0);
                    Debug.Assert(hookID != IntPtr.Zero, "Failed to set keyboardhook");
                }
            }
        }


        private IntPtr HookCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0)
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
                        string keyTextRaw;
                        keycodeMap.TryGetValue(keybStruct.vkCode, out keyTextRaw);
                        HookProc.InvokeSafely(this, new KeyboardHookArgs(keybStruct.vkCode, builder.ToString(), keyTextRaw));
                    }
                }
            }
            return WinAPI.CallNextHookEx(hookID, code, wParam, lParam);
        }


        ~KeyBoardHook()
        {
            Dispose(false);
            Debug.WriteLine("KeyboardHook Finalizer called");
        }

        private void Dispose(bool disposing)
        {
            if (isDisposed)
                return;
            WinAPI.UnhookWindowsHookEx(hookID);
            isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private sealed class KeycodeMap : Dictionary<int, string>
        {
            public KeycodeMap()
            {
                Add(8, "Backspace");
                Add(9, "Tab");
                Add(13, "Enter");
                Add(16, "Shift");
                Add(17, "Ctrl");
                Add(18, "Alt");
                Add(19, "Pause");
                Add(20, "Caps lock");
                Add(27, "Escape");
                Add(33, "Page up");
                Add(34, "Page down");
                Add(35, "End");
                Add(36, "Home");
                Add(37, "Left arrow");
                Add(38, "Up arrow");
                Add(39, "Right arrow");
                Add(40, "Down arrow");
                Add(45, "Insert");
                Add(46, "Delete");
                Add(91, "Left window");
                Add(92, "Right window");
                Add(93, "Select key");
                Add(96, "Numpad 0");
                Add(97, "Numpad 1");
                Add(98, "Numpad 2");
                Add(99, "Numpad 3");
                Add(100, "Numpad 4");
                Add(101, "Numpad 5");
                Add(102, "Numpad 6");
                Add(103, "Numpad 7");
                Add(104, "Numpad 8");
                Add(105, "Numpad 9");
                Add(106, "Multiply");
                Add(107, "Add");
                Add(109, "Subtract");
                Add(110, "Decimal point");
                Add(111, "Divide");
                Add(112, "F1");
                Add(113, "F2");
                Add(114, "F3");
                Add(115, "F4");
                Add(116, "F5");
                Add(117, "F6");
                Add(118, "F7");
                Add(119, "F8");
                Add(120, "F9");
                Add(121, "F10");
                Add(122, "F11");
                Add(123, "F12");
                Add(144, "Num lock");
                Add(145, "Scroll lock");
            }
        }
    }

}
