using System;
namespace AppsTracker.Tracking
{
    interface IWindowHelper
    {
        AppsTracker.Data.Utils.IAppInfo GetActiveWindowAppInfo();
        IntPtr GetActiveWindowHandle();
        string GetActiveWindowName();
    }
}
