using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Hooks
{
    public sealed class KeyboardHookArgs : EventArgs
    {
        private readonly int _keyCode;
        private readonly string _keyName;
        private readonly string _keyText;
        private readonly string _keyTextRaw;

        public int KeyCode { get { return _keyCode; } }
        public string KeyName { get { return _keyName; } }
        public string KeyText { get { return _keyText; } }
        public string KeyTextRaw { get { return _keyTextRaw; } }

        public KeyboardHookArgs(int keyCode)
        {
            _keyCode = keyCode;
            _keyName = System.Windows.Input.KeyInterop.KeyFromVirtualKey(keyCode).ToString();
        }

        public KeyboardHookArgs(int keyCode, string keyText, string keyTextRaw)
            : this(keyCode)
        {
            _keyText = keyText;
            _keyTextRaw = keyTextRaw ?? _keyName;
        }

    }
}
