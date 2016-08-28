using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Tracking;

namespace AppsTracker.Domain.Usages
{
    [Export(typeof(IUseCase<String, UsageOverview>))]
    public sealed class UsageOverviewUseCase : IUseCase<String, UsageOverview>
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public UsageOverviewUseCase(IRepository repository,
                                    ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public IEnumerable<UsageOverview> Get(String username)
        {
            IEnumerable<Usage> idles;
            IEnumerable<Usage> lockeds;
            IEnumerable<Usage> stoppeds;

            List<UsageOverview> collection = new List<UsageOverview>();

            var logins = repository.GetFiltered<Usage>(u => u.User.Name == username
                                                 && u.UsageStart >= trackingService.DateFrom
                                                 && u.UsageStart <= trackingService.DateTo
                                                 && u.UsageType == UsageTypes.Login);
            logins = BreakUsagesByDay(logins);

            var loginsGroupedByDay = logins.GroupBy(u => new
            {
                year = u.UsageStart.Year,
                month = u.UsageStart.Month,
                day = u.UsageStart.Day
            })
                                            .OrderBy(g => new DateTime(g.Key.year, g.Key.month, g.Key.day));

            foreach (var grp in loginsGroupedByDay)
            {
                var usageIDs = grp.Select(u => u.UsageID);

                idles = repository.GetFiltered<Usage>(u => u.SelfUsageID.HasValue
                                                  && usageIDs.Contains(u.SelfUsageID.Value)
                                                  && u.UsageType == UsageTypes.Idle);

                lockeds = repository.GetFiltered<Usage>(u => u.SelfUsageID.HasValue
                                                  && usageIDs.Contains(u.SelfUsageID.Value)
                                                  && u.UsageType == UsageTypes.Locked);

                stoppeds = repository.GetFiltered<Usage>(u => u.SelfUsageID.HasValue
                                                  && usageIDs.Contains(u.SelfUsageID.Value)
                                                  && u.UsageType == UsageTypes.Stopped);

                var day = new DateTime(grp.Key.year, grp.Key.month, grp.Key.day);

                var series = new UsageOverview()
                {
                    DateInstance = day,
                    Date = day.ToShortDateString()
                };

                var usages = new ObservableCollection<UsageSummary>();


                long idleTime = idles.Sum(l => l.GetDisplayedTicks(day));
                if (idleTime > 0)
                {
                    usages.Add(new UsageSummary()
                    {
                        Time = Math.Round(new TimeSpan(idleTime).TotalHours, 2),
                        UsageType = "Idle"
                    });
                }

                long lockedTime = lockeds.Sum(l => l.GetDisplayedTicks(day));
                if (lockedTime > 0)
                {
                    usages.Add(new UsageSummary()
                    {
                        Time = Math.Round(new TimeSpan(lockedTime).TotalHours, 2),
                        UsageType = "Computer locked"
                    });
                }

                long stoppedTime = stoppeds.Sum(l => l.GetDisplayedTicks(day));
                if (stoppedTime > 0)
                {
                    usages.Add(new UsageSummary()
                    {
                        Time = Math.Round(new TimeSpan(lockedTime).TotalHours, 2),
                        UsageType = "Stopped logging"
                    });
                }

                long loginTime = grp.Sum(l => l.GetDisplayedTicks(day)) - lockedTime - idleTime - stoppedTime;
                usages.Add(new UsageSummary()
                {
                    Time = Math.Round(new TimeSpan(loginTime).TotalHours, 2),
                    UsageType = "Work"
                });

                series.UsageCollection = usages;
                collection.Add(series);
            }

            return collection.OrderBy(c => c.DateInstance);
        }

        private IList<Usage> BreakUsagesByDay(IEnumerable<Usage> usages)
        {
            var tempUsages = new List<Usage>();

            foreach (var usage in usages)
            {
                if (usage.IsCurrent == false && usage.UsageEnd.Date == usage.UsageStart.Date)
                {
                    tempUsages.Add(usage);
                }
                else if (usage.IsCurrent == true)
                {
                    var startDaysInYear = GetDaysInYear(usage.UsageStart.Year);
                    var dayBegin = (startDaysInYear * usage.UsageStart.Year)
                                   + usage.UsageStart.DayOfYear - startDaysInYear;

                    var endDaysInYear = GetDaysInYear(DateTime.Now.Year);
                    var dayEnd = (endDaysInYear * DateTime.Now.Year)
                                + DateTime.Now.DayOfYear - endDaysInYear;

                    var span = dayEnd - dayBegin;

                    for (Int32 i = 0; i <= span; i++)
                    {
                        Usage tempUsage = new Usage(usage);
                        tempUsage.UsageStart = usage.GetDisplayedStart(usage.UsageStart.Date.AddDays(i));
                        tempUsage.UsageEnd = i == span ? DateTime.Now
                           : usage.GetDisplayedEnd(usage.UsageEnd.Date.AddDays(i + 1));
                        tempUsages.Add(tempUsage);
                    }
                }
                else
                {
                    var startDaysInYear = GetDaysInYear(usage.UsageStart.Year);
                    var dayBegin = (startDaysInYear * usage.UsageStart.Year)
                       + usage.UsageStart.DayOfYear - startDaysInYear;

                    var endDaysInYear = GetDaysInYear(usage.UsageEnd.Year);
                    var dayEnd = (endDaysInYear * usage.UsageEnd.Year)
                       + usage.UsageEnd.DayOfYear - endDaysInYear;

                    var span = dayEnd - dayBegin;

                    for (Int32 i = 0; i <= span; i++)
                    {
                        Usage tempUsage = new Usage(usage);
                        tempUsage.UsageStart = usage.GetDisplayedStart(usage.UsageStart.Date.AddDays(i));
                        tempUsage.UsageEnd = usage.GetDisplayedEnd(usage.UsageEnd.Date.AddDays(i));
                        tempUsages.Add(tempUsage);
                    }
                }
            }

            return tempUsages;
        }

        private Int32 GetDaysInYear(Int32 year)
        {
            var thisYear = new DateTime(year, 1, 1);
            var nextYear = new DateTime(year + 1, 1, 1);

            return (nextYear - thisYear).Days;
        }
    }
}
