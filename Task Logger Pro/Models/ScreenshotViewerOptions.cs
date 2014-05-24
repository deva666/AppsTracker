using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.Models
{
    public enum ScreenshotViewerRange : byte
    {
        SelectedLog,
        AllDayFromSelectedLog,
        DateRange
    }

    class ScreenshotViewerOptions
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public ScreenshotViewerOptions ScreenshotViewerOption { get; set; }

    }
}
