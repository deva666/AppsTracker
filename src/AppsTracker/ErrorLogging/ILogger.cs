using System;

namespace AppsTracker
{
    public interface ILogger
    {
        void Log(Exception ex);
    }
}
