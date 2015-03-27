﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Utils;

namespace AppsTracker.Hooks
{
    public sealed class WindowChangedArgs : EventArgs
    {

        public string WindowTitle { get; private set; }
        public IAppInfo AppInfo{ get; private set; }


        public WindowChangedArgs(string windowTitle, IAppInfo appInfo)
        {
            AppInfo = appInfo;
            WindowTitle = windowTitle;
        }

    }
}