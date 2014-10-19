using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.Hooks
{
    public sealed class KeyboardHookArgs : EventArgs
    {
        #region Fields

        int _keyCode;
        string _keyName;
        char _c;
        string _string;

        #endregion

        #region Properties

        public int KeyCode { get { return _keyCode; } }
        public string KeyName { get { return _keyName; } }
        public char Char { get { return _c; } }
        public string String { get { return _string; } }

        #endregion

        #region Constructors

        public KeyboardHookArgs(int keyCode)
        {
            _keyCode = keyCode;
            _keyName = System.Windows.Input.KeyInterop.KeyFromVirtualKey(keyCode).ToString();
        }

        public KeyboardHookArgs(int keyCode, char c)
            : this(keyCode)
        {
            _c = c;
        }

        public KeyboardHookArgs(int keyCode, string str)
            : this(keyCode)
        {
            _string = str;
        }

        #endregion
    }
}
