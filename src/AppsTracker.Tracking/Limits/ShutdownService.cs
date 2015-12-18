using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using AppsTracker.Common.Logging;
using AppsTracker.Data.Models;

namespace AppsTracker.Tracking.Limits
{
    [Export(typeof(IShutdownService))]
    internal sealed class ShutdownService : IShutdownService
    {
        private readonly ILogger logger;

        [ImportingConstructor]
        public ShutdownService(ILogger logger)
        {
            this.logger = logger;
        }

        public void Shutdown(string appName)
        {
            try
            {
                var processes = Process.GetProcessesByName(appName);
                foreach (var proc in processes)
                {
                    proc.Kill();
                }
            }
            catch (Exception fail)
            {
                logger.Log(fail);
            }
        }
    }
}
