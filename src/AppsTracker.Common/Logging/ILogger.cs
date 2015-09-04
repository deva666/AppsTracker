using System;

namespace AppsTracker.Common.Logging
{
    public interface ILogger
    {
        void Log(Exception ex);
    }
}
