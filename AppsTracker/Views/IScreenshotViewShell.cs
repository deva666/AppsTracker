using System.Collections.Generic;
using AppsTracker.Widgets;

namespace AppsTracker.Views
{
    public interface IScreenshotViewShell : IShell
    {
        IEnumerable<AppsTracker.Data.Models.Screenshot> Screenshots { get; set; }
    }
}
