using System;

namespace AppsTracker.Tracking.Hooks
{
    public class WinChangedArgs : System.EventArgs
    {
        public String Title { get; private set; }
        public IntPtr Handle { get; private set; }

        public WinChangedArgs(String title, IntPtr handle)
        {
            Title = title;
            Handle = handle;
        }
    }
}
