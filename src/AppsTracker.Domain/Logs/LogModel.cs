using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Domain.Model;
using AppsTracker.Domain.Screenshots;
using AppsTracker.Domain.Usages;
using AppsTracker.Domain.Windows;

namespace AppsTracker.Domain.Logs
{
    public sealed class LogModel : SelectableBase
    {
        internal LogModel(Log log)
        {
            DateCreated = log.DateCreated;
            DateEnded = log.DateEnded;
            UtcDateCreated = log.UtcDateCreated;
            UtcDateEnded = log.UtcDateEnded;
            Finished = log.Finished;
            Window = new WindowModel(log.Window);
            Screenshots = log.Screenshots?.Select(s => new ScreenshotModel(this, s));
        }

        public long Duration
        {
            get
            {
                return Finished ? UtcDateEnded.Ticks - UtcDateCreated.Ticks : DateTime.UtcNow.Ticks - UtcDateCreated.Ticks;
            }
        }

        public bool Finished { get; }

        public DateTime DateCreated { get; }

        public DateTime DateEnded { get; }

        public DateTime UtcDateCreated { get; }

        public DateTime UtcDateEnded { get; }

        public IEnumerable<ScreenshotModel> Screenshots { get; }

        public WindowModel Window { get; }

        public UsageModel Usage { get; }
    }
}
