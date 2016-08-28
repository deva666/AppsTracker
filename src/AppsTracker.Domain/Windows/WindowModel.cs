using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Domain.Apps;
using AppsTracker.Domain.Logs;

namespace AppsTracker.Domain.Windows
{
    public sealed class WindowModel
    {
        public WindowModel(Window window)
        {
            Title = window.Title;
            Application = new AppModel(window.Application);
        }

        public string Title { get; }

        public AppModel Application { get; set; }
        public LogModel Log { get; set; }

    }
}
