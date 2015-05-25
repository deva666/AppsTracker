using System;

namespace AppsTracker.Tracking.Helpers
{
    interface IScreenshotFactory
    {
        AppsTracker.Data.Models.Screenshot CreateScreenshot();
    }
}
