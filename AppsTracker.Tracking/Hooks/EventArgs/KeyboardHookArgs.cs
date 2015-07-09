using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Tracking.Hooks
{
    public sealed class KeyboardHookArgs : EventArgs
    {
        private readonly int _keyCode;
        private readonly string _keyText;
        private readonly string _keyTextRaw;

        public int KeyCode { get { return _keyCode; } }
        public string KeyText { get { return _keyText; } }
        public string KeyTextRaw { get { return _keyTextRaw; } }

        public KeyboardHookArgs(int keyCode)
        {
            _keyCode = keyCode;            
        }

        public KeyboardHookArgs(int keyCode, string keyText, string keyTextRaw)
            : this(keyCode)
        {
            _keyText = keyText;
            _keyTextRaw = keyTextRaw;
        }

    }
}
