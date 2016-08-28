using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Domain.Apps;
using AppsTracker.Domain.Logs;

namespace AppsTracker.Domain.Windows
{
    public sealed class WindowModel
    {
        public TimeSpan Duration
        {
            get
            {
                return GetWindowDuration();
            }
        }

        private TimeSpan GetWindowDuration()
        {
            long ticks = 0;
            foreach (var log in this.Logs)
            {
                ticks += log.Duration;
            }
            return new TimeSpan(ticks);
        }

        public string Title { get; set; }

        public AppModel Application { get; set; }
        public ICollection<LogModel> Logs { get; set; }

    }
}
