using System;

namespace AppsTracker.Logging.Helpers
{
    interface IScreenshotFactory
    {
        AppsTracker.Data.Models.Screenshot CreateScreenshot();
    }
}
