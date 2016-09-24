using System;
using System.Collections.Generic;
using System.Linq;
using AppsTracker.Data.Models;
using AppsTracker.Domain.Apps;

namespace AppsTracker.Domain.Screenshots
{
    public sealed class ScreenshotModel
    {
        internal ScreenshotModel(Log log)
        {
            DateCreated = log.DateCreated;
            WindowTitle = log.Window.Title;
            AppName = log.Window.Application.Name;
            AppModel = new AppModel(log.Window.Application);
            Images = log.Screenshots?.Select(s => new Image(log.Window.Application.Name, s)).ToList();
        }

        public DateTime DateCreated { get; }

        public IList<Image> Images { get; }

        public String AppName { get; }

        public AppModel AppModel { get; }

        public String WindowTitle { get; }
    }
}
