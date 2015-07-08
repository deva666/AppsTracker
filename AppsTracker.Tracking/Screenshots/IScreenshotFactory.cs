using System;

namespace AppsTracker.Tracking.Helpers
{
    public interface IScreenshotFactory
    {
        AppsTracker.Data.Models.Screenshot CreateScreenshot();
    }
}
