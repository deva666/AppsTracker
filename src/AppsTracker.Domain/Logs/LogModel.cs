using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Domain.Model;
using AppsTracker.Domain.Screenshots;
using AppsTracker.Domain.Usages;
using AppsTracker.Domain.Windows;

namespace AppsTracker.Domain.Logs
{
    public sealed class LogModel : SelectableBase
    {
        public long Duration
        {
            get
            {
                return Finished ? UtcDateEnded.Ticks - UtcDateCreated.Ticks : DateTime.UtcNow.Ticks - UtcDateCreated.Ticks;
            }
        }

        public bool Finished { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateEnded { get; set; }

        public DateTime UtcDateCreated { get; set; }

        public DateTime UtcDateEnded { get; set; }

        public ICollection<ScreenshotModel> Screenshots { get; set; }

        public WindowModel Window { get; set; }

        public UsageModel Usage { get; set; }
    }
}
