using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.Hooks
{
    public sealed class WinEventArgs : EventArgs
    {

        #region Properties

        public string WindowTitle { get; private set; }
        public ProcessInfo ProcessInfo { get; private set; }

        #endregion

        #region Constructor

        public WinEventArgs(string windowTitle, Process process)
        {
            ProcessInfo = ProcessInfo.GetProcessInfo(process);
            WindowTitle = windowTitle;
        }

        #endregion
    }
}
