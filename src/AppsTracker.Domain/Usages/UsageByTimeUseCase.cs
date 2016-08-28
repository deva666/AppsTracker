using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Tracking;

namespace AppsTracker.Domain.Usages
{
    [Export(typeof(IUseCase<DateTime, UsageByTime>))]
    public sealed class UsageByTimeUseCase : IUseCase<DateTime, UsageByTime>
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public UsageByTimeUseCase(IRepository repository,
                                  ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public IEnumerable<UsageByTime> Get(DateTime selectedDate)
        {
            var fromDay = selectedDate.Date;
            var nextDay = fromDay.AddDays(1d);
            var today = DateTime.Now.Date;

            var logins = repository.GetFiltered<Usage>(u => u.User.UserID == trackingService.SelectedUserID
                                            && ((u.UsageStart >= fromDay && u.UsageStart <= nextDay)
                                                    || (u.IsCurrent && u.UsageStart < fromDay && today >= fromDay)
                                                    || (u.IsCurrent == false && u.UsageStart <= fromDay && u.UsageEnd >= fromDay))
                                            && u.UsageType == UsageTypes.Login);

            var usageIDs = logins.Select(u => u.UsageID).ToList();

            var allUsages = repository.GetFiltered<Usage>(u => u.SelfUsageID.HasValue
                                                           && usageIDs.Contains(u.SelfUsageID.Value));

            var lockedUsages = allUsages.Where(u => u.UsageType == UsageTypes.Locked);
            var idleUsages = allUsages.Where(u => u.UsageType == UsageTypes.Idle);
            var stoppedUsages = allUsages.Where(u => u.UsageType == UsageTypes.Stopped);

            var usagesByTime = new List<UsageByTime>();

            foreach (var login in logins)
            {
                var series = new UsageByTime() { Time = login.GetDisplayedStart(fromDay).ToString("HH:mm:ss") };
                var usageSummaries = new ObservableCollection<UsageSummary>();

                var tempIdles = idleUsages.Where(u => u.SelfUsageID == login.UsageID);
                var tempLockeds = lockedUsages.Where(u => u.SelfUsageID == login.UsageID);
                var tempStopppeds = stoppedUsages.Where(u => u.SelfUsageID == login.UsageID);

                long idleDuration = tempIdles.Sum(l => l.GetDisplayedTicks(fromDay));
                if (idleDuration > 0)
                {
                    usageSummaries.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(idleDuration).TotalHours, 2), UsageType = "Idle" });
                }

                long lockedDuration = tempLockeds.Sum(l => l.GetDisplayedTicks(fromDay));
                if (lockedDuration > 0)
                {
                    usageSummaries.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(lockedDuration).TotalHours, 2), UsageType = "Computer locked" });
                }

                long stoppedDuration = tempStopppeds.Sum(l => l.GetDisplayedTicks(fromDay));
                if (stoppedDuration > 0)
                {
                    usageSummaries.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(lockedDuration).TotalHours, 2), UsageType = "Stopped logging" });
                }

                long loginDuration = login.GetDisplayedTicks(fromDay) - lockedDuration - idleDuration - stoppedDuration;
                usageSummaries.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(loginDuration).TotalHours, 2), UsageType = "Work" });

                series.UsageSummaryCollection = usageSummaries;

                usagesByTime.Add(series);
            }

            if (logins.Count() > 1)
            {
                var seriesTotal = new UsageByTime() { Time = "TOTAL" };
                var usageSummaries = new ObservableCollection<UsageSummary>();

                long totalIdleDuration = idleUsages.Sum(l => l.GetDisplayedTicks(fromDay));
                if (totalIdleDuration > 0)
                {
                    usageSummaries.Add(new UsageSummary()
                    {
                        Time = Math.Round(new TimeSpan(totalIdleDuration).TotalHours, 2),
                        UsageType = "Idle"
                    });
                }

                long totalLockedDuration = lockedUsages.Sum(l => l.GetDisplayedTicks(fromDay));
                if (totalLockedDuration > 0)
                {
                    usageSummaries.Add(new UsageSummary()
                    {
                        Time = Math.Round(new TimeSpan(totalLockedDuration).TotalHours, 2),
                        UsageType = "Computer locked"
                    });
                }

                long totalStoppedDuration = stoppedUsages.Sum(l => l.GetDisplayedTicks(fromDay));
                if (totalStoppedDuration > 0)
                {
                    usageSummaries.Add(new UsageSummary()
                    {
                        Time = Math.Round(new TimeSpan(totalStoppedDuration).TotalHours, 2),
                        UsageType = "Stopped logging"
                    });
                }

                long totalLoginDuration = logins.Sum(l => l.GetDisplayedTicks(fromDay)) - totalLockedDuration - totalIdleDuration;
                usageSummaries.Add(new UsageSummary()
                {
                    Time = Math.Round(new TimeSpan(totalLoginDuration).TotalHours, 2),
                    UsageType = "Work"
                });

                seriesTotal.UsageSummaryCollection = usageSummaries;

                usagesByTime.Add(seriesTotal);
            }

            return usagesByTime;
        }
    }
}
