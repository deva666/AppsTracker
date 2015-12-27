using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;

namespace AppsTracker.Tracking.Limits
{
    [Export(typeof(IAppDurationCalc))]
    public class AppDurationCalc : IAppDurationCalc
    {
        private readonly IDataService dataService;

        [ImportingConstructor]
        public AppDurationCalc(IDataService dataService)
        {
            this.dataService = dataService;
        }

        public async Task<long> GetDuration(Aplication app, LimitSpan span)
        {
            switch (span)
            {
                case LimitSpan.Day:
                    return await SumDuration(app, l => l.DateCreated >= DateTime.Now.Date);
                case LimitSpan.Week:
                    DateTime today = DateTime.Today;
                    int delta = DayOfWeek.Monday - today.DayOfWeek;
                    if (delta > 0)
                        delta -= 7;
                    DateTime weekBegin = today.AddDays(delta);
                    DateTime weekEnd = weekBegin.AddDays(6);
                    return await SumDuration(app, l => l.DateCreated >= weekBegin && l.DateCreated <= weekEnd);
                default:
                    return -1;
            }
        }

        private async Task<long> SumDuration(Aplication app, Func<Log, bool> filter)
        {
            var appsList = await dataService.GetFilteredAsync<Aplication>(a => a.ApplicationID == app.ApplicationID
                                                                           && a.Windows.SelectMany(w => w.Logs).Any(filter),
                                                                         a => a.Windows.Select(w => w.Logs));

            return appsList.SelectMany(a => a.Windows)
                           .SelectMany(w => w.Logs)
                           .Sum(l => l.Duration);
        }
    }
}
