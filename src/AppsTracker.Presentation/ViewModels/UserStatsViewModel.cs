#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Data.Service;
using AppsTracker.Common.Communication;
using AppsTracker.Tracking;
using System.Collections.ObjectModel;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class UserStatsViewModel : ViewModelBase
    {
        private readonly IDataService dataService;
        private readonly ITrackingService trackingService;
        private readonly IMediator mediator;

        public override string Title
        {
            get { return "USERS"; }
        }


        public object SelectedItem { get; set; }


        private UserLoggedTime selectedUser;

        public UserLoggedTime SelectedUser
        {
            get { return selectedUser; }
            set
            {
                SetPropertyValue(ref selectedUser, value);
                if (selectedUser != null)
                    dailyUsageList.Reload();
            }
        }


        public UsageModel UsageModel { get; set; }


        private readonly AsyncProperty<IEnumerable<UserLoggedTime>> usersList;

        public AsyncProperty<IEnumerable<UserLoggedTime>> UsersList
        {
            get { return usersList; }
        }


        private readonly AsyncProperty<IEnumerable<UsageOverview>> dailyUsageList;

        public AsyncProperty<IEnumerable<UsageOverview>> DailyUsageList
        {
            get { return dailyUsageList; }
        }


        private ICommand returnFromDetailedViewCommand;

        public ICommand ReturnFromDetailedViewCommand
        {
            get
            {
                return returnFromDetailedViewCommand ??
                  (returnFromDetailedViewCommand = new DelegateCommand(ReturnFromDetailedView));
            }
        }


        [ImportingConstructor]
        public UserStatsViewModel(IDataService dataService,
                                  ITrackingService trackingService,
                                  IMediator mediator)
        {
            this.dataService = dataService;
            this.trackingService = trackingService;
            this.mediator = mediator;

            usersList = new TaskRunner<IEnumerable<UserLoggedTime>>(GetContent, this);
            dailyUsageList = new TaskRunner<IEnumerable<UsageOverview>>(GetSubContent, this);

            this.mediator.Register(MediatorMessages.REFRESH_LOGS, new Action(ReloadAll));
        }


        private void ReloadAll()
        {
            usersList.Reload();
            dailyUsageList.Reload();
        }


        private IEnumerable<UserLoggedTime> GetContent()
        {
            var logins = dataService.GetFiltered<Usage>(u => u.UsageStart >= trackingService.DateFrom
                                                    && u.UsageStart <= trackingService.DateTo
                                                    && u.UsageType == UsageTypes.Login,
                                                    u => u.User);

            return logins.GroupBy(u => u.User.Name)
                            .Select(g => new UserLoggedTime
                            {
                                Username = g.Key,
                                LoggedInTime = Math.Round(new TimeSpan(g.Sum(l => l.Duration.Ticks)).TotalHours, 1)
                            });
        }


        private IEnumerable<UsageOverview> GetSubContent()
        {
            var user = SelectedUser;
            if (user == null)
                return null;

            IEnumerable<Usage> idles;
            IEnumerable<Usage> lockeds;
            IEnumerable<Usage> stoppeds;

            List<UsageOverview> collection = new List<UsageOverview>();

            var logins = dataService.GetFiltered<Usage>(u => u.User.Name == user.Username
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

                idles = dataService.GetFiltered<Usage>(u => u.SelfUsageID.HasValue
                                                  && usageIDs.Contains(u.SelfUsageID.Value)
                                                  && u.UsageType == UsageTypes.Idle);

                lockeds = dataService.GetFiltered<Usage>(u => u.SelfUsageID.HasValue
                                                  && usageIDs.Contains(u.SelfUsageID.Value)
                                                  && u.UsageType == UsageTypes.Locked);

                stoppeds = dataService.GetFiltered<Usage>(u => u.SelfUsageID.HasValue
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


        private void ReturnFromDetailedView()
        {
            SelectedUser = null;
        }
    }
}
