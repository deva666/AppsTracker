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
using AppsTracker.DAL.Service;

namespace AppsTracker.Pages.ViewModels
{
    internal sealed class Statistics_usersViewModel : ViewModelBase, IChildVM, ICommunicator
    {
        #region Fields

        AllUsersModel _allUsersModel;

        IEnumerable<AllUsersModel> _allUsersList;

        IEnumerable<UsageTypeSeries> _dailyLogins;

        ICommand _returnFromDetailedViewCommand;

        IChartService _service;

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
            _service = ServiceFactory.Get<IChartService>();
        }

        #region Loader Methods

        public async void LoadContent()
        {
            await LoadAsync(GetContent, a => AllUsersList = a);
            LoadSubContent();
            IsContentLoaded = true;
        }

        private async void LoadSubContent()
        {
            if (AllUsersModel == null)
                return;

            DailyLogins = null;
            await LoadAsync(GetSubContent, d => DailyLogins = d);
        }

        private IEnumerable<AllUsersModel> GetContent()
        {
            return _service.GetAllUsers(Globals.Date1, Globals.Date2);
        }

        private IEnumerable<UsageTypeSeries> GetSubContent()
        {
            var model = AllUsersModel;
            if (model == null)
                return null;

            return _service.GetUsageSeries(model.Username, Globals.Date1, Globals.Date2);
        }

        #endregion

        private void ReturnFromDetailedView()
        {
            AllUsersModel = null;
        }
    }
}
