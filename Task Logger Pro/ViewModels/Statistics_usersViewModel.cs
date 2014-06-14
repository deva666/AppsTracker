using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.DAL;
using AppsTracker.Models.ChartModels;
using AppsTracker.Models.EntityModels;
using Task_Logger_Pro.Controls;
using Task_Logger_Pro.MVVM;
using System.Data.Entity;
using System.Collections.ObjectModel;

namespace Task_Logger_Pro.Pages.ViewModels
{
    class Statistics_usersViewModel : ViewModelBase, IChildVM, IWorker, ICommunicator
    {
        #region Fields

        bool _working;

        AllUsersModel _allUsersModel;

        List<AllUsersModel> _allUsersList;

        List<UsageTypeSeries> _dailyLogins;

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
        public bool Working
        {
            get
            {
                return _working;
            }
            set
            {
                _working = value;
                PropertyChanging("Working");
            }
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
                    DailyLogins = LoadSubContentAsync().Result;
            }
        }
        public UsageModel UsageModel
        {
            get;
            set;
        }
        public List<AllUsersModel> AllUsersList
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
        public List<UsageTypeSeries> DailyLogins
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
                if (_returnFromDetailedViewCommand == null)
                    _returnFromDetailedViewCommand = new DelegateCommand(ReturnFromDetailedView);
                return _returnFromDetailedViewCommand;
            }
        }

        public Mediator Mediator
        {
            get { return Mediator.Instance; }
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
            AllUsersList = await LoadContentAsync();
            if (AllUsersModel != null)
                DailyLogins = await LoadSubContentAsync();
            Working = false;
            IsContentLoaded = true;
        }

        private Task<List<AllUsersModel>> LoadContentAsync()
        {
            return Task<List<AllUsersModel>>.Factory.StartNew(() =>
            {
                using (var context = new AppsEntities())
                {
                    string loginType = UsageTypes.Login.ToString();

                    return (from l in context.Usages.AsNoTracking()
                            where l.UsageStart >= Globals.Date1
                            && l.UsageStart <= Globals.Date2
                            && l.UsageType.UType == loginType
                            group l by l.User.Name into g
                            select g).ToList()
                                    .Select(g => new AllUsersModel
                                    {
                                        Username = g.Key,
                                        LoggedInTime = Math.Round(new TimeSpan(g.Sum(l => l.Duration.Ticks)).TotalHours, 1)
                                    }).ToList();

                }
            }, System.Threading.CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private async Task LoadSubContent()
        {
            if (AllUsersModel == null)
                return;
            _dailyLogins = null;
            PropertyChanging("DailyLogins");
            Working = true;
            _dailyLogins = await LoadSubContentAsync();
            Working = false;
            PropertyChanging("DailyLogins");
        }

        private Task<List<UsageTypeSeries>> LoadSubContentAsync()
        {
            return Task<List<UsageTypeSeries>>.Run(() =>
            {
                //using ( var context = new AppsEntities( ) )
                //{
                //    string loginType = UsageTypes.Login.ToString();

                //    var selectedUser = context.Users.FirstOrDefault( u => u.Name == AllUsersModel.Username );

                //    if ( selectedUser == null )
                //        throw new NullReferenceException( "selectedUser" );
                //    return ( from l in context.Usages.AsNoTracking( )
                //             where l.UserID == selectedUser.UserID
                //             && l.UsageStart >= Globals.Date1
                //             && l.UsageStart <= Globals.Date2
                //             && l.UsageType.UType == loginType
                //             group l by new { year = l.UsageStart.Year, month = l.UsageStart.Month, day = l.UsageStart.Day } into g
                //             orderby g.Key.year, g.Key.month, g.Key.day
                //             select g ).ToList( )
                //            .Select( g => new UsageModel
                //            {
                //                Date = new DateTime( g.Key.year, g.Key.month, g.Key.day ).ToShortDateString( ),
                //                Count = Math.Round( g.Sum( l => l.Duration.TotalHours ), 1 )
                //            } ).ToList( );
                //}

                string usageLogin = UsageTypes.Login.ToString();
                string usageIdle = UsageTypes.Idle.ToString();
                string usageLocked = UsageTypes.Locked.ToString();
                string usageStopped = UsageTypes.Stopped.ToString();

                List<Usage> idles;
                List<Usage> lockeds;
                List<Usage> stoppeds;

                List<UsageTypeSeries> collection = new List<UsageTypeSeries>();

                using (var context = new AppsEntities())
                {
                    var groupedLogins = (from u in context.Users.AsNoTracking()
                                         join l in context.Usages.AsNoTracking() on u.UserID equals l.UserID
                                         where u.UserID == Globals.SelectedUserID
                                         && l.UsageStart >= Globals.Date1
                                         && l.UsageStart <= Globals.Date2
                                         && l.UsageType.UType == usageLogin
                                         group l by new { year = l.UsageStart.Year, month = l.UsageStart.Month, day = l.UsageStart.Day } into g
                                         orderby g.Key.year, g.Key.month, g.Key.day
                                         select g).ToList();

                    foreach (var grp in groupedLogins)
                    {
                        var usageIDs = grp.Select(u => u.UsageID);

                        idles = context.Usages.Where(u => u.SelfUsageID.HasValue && usageIDs.Contains(u.SelfUsageID.Value) && u.UsageType.UType == usageIdle).ToList();

                        lockeds = context.Usages.Where(u => u.SelfUsageID.HasValue && usageIDs.Contains(u.SelfUsageID.Value) && u.UsageType.UType == usageLocked).ToList();

                        stoppeds = context.Usages.Where(u => u.SelfUsageID.HasValue && usageIDs.Contains(u.SelfUsageID.Value) && u.UsageType.UType == usageStopped).ToList();

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
                }
                return collection;
            });
        }

        #endregion

        private void ReturnFromDetailedView()
        {
            AllUsersModel = null;
        }
    }
}
