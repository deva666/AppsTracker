using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.DAL;
using AppsTracker.Models.ChartModels;
using AppsTracker.Models.EntityModels;
using AppsTracker.Controls;
using AppsTracker.MVVM;
using System.Data.Entity;
using System.Collections.ObjectModel;
using System.Diagnostics;
using AppsTracker.DAL.Repos;

namespace AppsTracker.Pages.ViewModels
{
    internal sealed class Statistics_usersViewModel : ViewModelBase, IChildVM, ICommunicator
    {
        #region Fields

        AllUsersModel _allUsersModel;

        IEnumerable<AllUsersModel> _allUsersList;

        IEnumerable<UsageTypeSeries> _dailyLogins;

        ICommand _returnFromDetailedViewCommand;

        #endregion

        #region Properties

        public string Title
        {
            get
            {
                return "USERS";
            }
        }
        public object SelectedItem
        {
            get;
            set;
        }
        public bool IsContentLoaded
        {
            get;
            private set;
        }
        
        public AllUsersModel AllUsersModel
        {
            get
            {
                return _allUsersModel;
            }
            set
            {
                _allUsersModel = value;
                PropertyChanging("AllUsersModel");
                if (_allUsersModel != null)
                    LoadSubContent();
            }
        }
        public UsageModel UsageModel
        {
            get;
            set;
        }
        public IEnumerable<AllUsersModel> AllUsersList
        {
            get
            {
                return _allUsersList;
            }
            set
            {
                _allUsersList = value;
                PropertyChanging("AllUsersList");
            }
        }
        public IEnumerable<UsageTypeSeries> DailyLogins
        {
            get
            {
                return _dailyLogins;
            }
            set
            {
                _dailyLogins = value;
                PropertyChanging("DailyLogins");
            }
        }
        public SettingsProxy UserSettings
        {
            get
            {
                return App.UzerSetting;
            }
        }
        public ICommand ReturnFromDetailedViewCommand
        {
            get
            {
                return _returnFromDetailedViewCommand == null ? _returnFromDetailedViewCommand = new DelegateCommand(ReturnFromDetailedView) : _returnFromDetailedViewCommand;
            }
        }

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }

        #endregion

        public Statistics_usersViewModel()
        {
            Mediator.Register(MediatorMessages.RefreshLogs, new Action(LoadContent));
        }

        #region Loader Methods

        public async void LoadContent()
        {
            Working = true;
            AllUsersList = await GetContentAsync().ConfigureAwait(false);
            if (AllUsersModel != null)
                DailyLogins = await LoadSubContentAsync();
            Working = false;
            IsContentLoaded = true;
        }

        private async void LoadSubContent()
        {
            if (AllUsersModel == null)
                return;
            DailyLogins = null;
            Working = true;
            DailyLogins = await LoadSubContentAsync().ConfigureAwait(false);
            Working = false;
        }

        private async Task<IEnumerable<AllUsersModel>> GetContentAsync()
        {
            string loginType = UsageTypes.Login.ToString();
            var logins = await UsageRepo.Instance.GetFilteredAsync(u => u.UsageStart >= Globals.Date1
                                                                        && u.UsageStart <= Globals.Date2
                                                                        && u.UsageType.UType == loginType
                                                                        , u => u.User)
                                                .ConfigureAwait(false);

            return logins.GroupBy(u => u.User.Name)
                            .Select(g => new AllUsersModel
                                                        {
                                                            Username = g.Key,
                                                            LoggedInTime = Math.Round(new TimeSpan(g.Sum(l => l.Duration.Ticks)).TotalHours, 1)
                                                        });
        }

        private async Task<List<UsageTypeSeries>> LoadSubContentAsync()
        {
            string usageLogin = UsageTypes.Login.ToString();
            string usageIdle = UsageTypes.Idle.ToString();
            string usageLocked = UsageTypes.Locked.ToString();
            string usageStopped = UsageTypes.Stopped.ToString();

            IEnumerable<Usage> idles;
            IEnumerable<Usage> lockeds;
            IEnumerable<Usage> stoppeds;

            List<UsageTypeSeries> collection = new List<UsageTypeSeries>();
            //wrong, selected user
            var logins = await UsageRepo.Instance.GetFilteredAsync(u => u.User.UserID == Globals.SelectedUserID
                                                                 && u.UsageStart >= Globals.Date1
                                                                 && u.UsageStart <= Globals.Date2
                                                                 && u.UsageType.UType == usageLogin)
                                     .ConfigureAwait(false);

            var groupedLogins = logins.GroupBy(u => new
                                                    {
                                                        year = u.UsageStart.Year,
                                                        month = u.UsageStart.Month,
                                                        day = u.UsageStart.Day
                                                    })
                                        .OrderBy(g => new DateTime(g.Key.year, g.Key.month, g.Key.day));

            foreach (var grp in groupedLogins)
            {
                var usageIDs = grp.Select(u => u.UsageID);

                var idlesTask = UsageRepo.Instance.GetFilteredAsync(u => u.SelfUsageID.HasValue
                                                                        && usageIDs.Contains(u.SelfUsageID.Value)
                                                                        && u.UsageType.UType == usageIdle);

                var lockedTask = UsageRepo.Instance.GetFilteredAsync(u => u.SelfUsageID.HasValue
                                                                            && usageIDs.Contains(u.SelfUsageID.Value)
                                                                            && u.UsageType.UType == usageLocked);

                var stoppedTask = UsageRepo.Instance.GetFilteredAsync(u => u.SelfUsageID.HasValue
                                                                            && usageIDs.Contains(u.SelfUsageID.Value)
                                                                            && u.UsageType.UType == usageStopped);

                await Task.WhenAll(idlesTask, lockedTask, stoppedTask)
                        .ConfigureAwait(false);

                idles = idlesTask.Result;
                lockeds = lockedTask.Result;
                stoppeds = stoppedTask.Result;

                UsageTypeSeries series = new UsageTypeSeries() { Date = new DateTime(grp.Key.year, grp.Key.month, grp.Key.day).ToShortDateString() };

                ObservableCollection<UsageTypeModel> observableCollection = new ObservableCollection<UsageTypeModel>();

                long idleTime = 0;
                long lockedTime = 0;
                long loginTime = 0;
                long stoppedTime = 0;

                if (idles.Count() > 0)
                {
                    idleTime = idles.Sum(l => l.Duration.Ticks);
                    observableCollection.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(idleTime).TotalHours, 2), UsageType = usageIdle });
                }

                if (lockeds.Count() > 0)
                {
                    lockedTime = lockeds.Sum(l => l.Duration.Ticks);
                    observableCollection.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(lockedTime).TotalHours, 2), UsageType = "Computer locked" });
                }


                if (stoppeds.Count() > 0)
                {
                    stoppedTime = stoppeds.Sum(l => l.Duration.Ticks);
                    observableCollection.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(lockedTime).TotalHours, 2), UsageType = "Stopped logging" });
                }

                loginTime = grp.Sum(l => l.Duration.Ticks) - lockedTime - idleTime - stoppedTime;
                observableCollection.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(loginTime).TotalHours, 2), UsageType = "Work" });

                series.DailyUsageTypeCollection = observableCollection;

                collection.Add(series);
            }
            return collection;
        }

        #endregion

        private void ReturnFromDetailedView()
        {
            AllUsersModel = null;
        }
    }
}
