using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Task_Logger_Pro.Controls;
using Task_Logger_Pro.Models;
using Task_Logger_Pro.MVVM;

namespace Task_Logger_Pro.Pages.ViewModels
{
    class Statistics_usersViewModel : ViewModelBase, IChildVM, IWorker, ICommunicator
    {
        #region Fields

        bool _working;

        AllUsersModel _allUsersModel;

        WeakReference _weakCollection = new WeakReference(null);

        List<UsageModel> _dailyLogins;

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
            get
            {
                return _weakCollection.Target != null;
            }
        }
        public bool Working
        {
            get
            {
                return _working;
            }
            set
            {
                _working = value; PropertyChanging("Working");
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
                _allUsersModel = value; PropertyChanging("AllUsersModel");
                LoadSubContent();
            }
        }
        public UsageModel UsageModel
        {
            get;
            set;
        }
        public WeakReference WeakCollection
        {
            get
            {
                if (_weakCollection.Target == null && !Working)
                    LoadContent();
                return _weakCollection;
            }
        }
        public List<UsageModel> DailyLogins
        {
            get
            {
                return _dailyLogins;
            }
        }
        public UzerSetting UserSettings
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
            WeakCollection.Target = await LoadContentAsync();
            Working = false;
            PropertyChanging("WeakCollection");
        }

        private Task<List<AllUsersModel>> LoadContentAsync( )
        {
            return Task<List<AllUsersModel>>.Factory.StartNew( ( ) =>
            {
                using ( var context = new AppsEntities1( ) )
                {
                    string loginType = UsageTypes.Login.ToString();

                    return ( from l in context.Usages.AsNoTracking( )
                             where l.UsageStart >= Globals.Date1
                             && l.UsageStart <= Globals.Date2 
                             && l.UsageType.UType == loginType
                             group l by l.User.Name into g
                             select g ).ToList( )
                                    .Select( g => new AllUsersModel
                                    {
                                        Username = g.Key,
                                        LoggedInTime = Math.Round( new TimeSpan( g.Sum( l => l.Duration.Ticks ) ).TotalHours, 1 )
                                    } ).ToList( );

                }
            }, System.Threading.CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default );
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

        private Task<List<UsageModel>> LoadSubContentAsync( )
        {
            return Task<List<UsageModel>>.Run( ( ) =>
            {
                using ( var context = new AppsEntities1( ) )
                {
                    string loginType = UsageTypes.Login.ToString();

                    var selectedUser = context.Users.FirstOrDefault( u => u.Name == AllUsersModel.Username );

                    if ( selectedUser == null )
                        throw new NullReferenceException( "Could not find user" );
                    return ( from l in context.Usages.AsNoTracking( )
                             where l.UserID == selectedUser.UserID
                             && l.UsageStart >= Globals.Date1
                             && l.UsageStart <= Globals.Date2
                             && l.UsageType.UType == loginType
                             group l by new { year = l.UsageStart.Year, month = l.UsageStart.Month, day = l.UsageStart.Day } into g
                             orderby g.Key.year, g.Key.month, g.Key.day
                             select g ).ToList( )
                            .Select( g => new UsageModel
                            {
                                Date = new DateTime( g.Key.year, g.Key.month, g.Key.day ).ToShortDateString( ),
                                Count = Math.Round( g.Sum( l => l.Duration.TotalHours ), 1 )
                            } ).ToList( );
                }
            } );
        }

        #endregion

        private void ReturnFromDetailedView()
        {
            AllUsersModel = null;
        }
    }
}
