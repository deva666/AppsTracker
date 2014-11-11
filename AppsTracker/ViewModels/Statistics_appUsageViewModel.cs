using System;
using System.Collections.Generic;
using System.Windows.Input;

using AppsTracker.MVVM;
using AppsTracker.Models.ChartModels;
using AppsTracker.DAL.Service;


namespace AppsTracker.Pages.ViewModels
{
    internal sealed class Statistics_appUsageViewModel : ViewModelBase, IChildVM, ICommunicator
    {
        #region Fields

        MostUsedAppModel _mostUsedAppModel;

        IEnumerable<MostUsedAppModel> _mostUsedAppsList;

        IEnumerable<DailyAppModel> _dailyAppList;

        ICommand _returnFromDetailedViewCommand;

        IChartService _service;

        #endregion

        #region Properties

        public string Title
        {
            get
            {
                return "APPS";
            }
        }

        public bool IsContentLoaded
        {
            get;
            private set;
        }

        public MostUsedAppModel MostUsedAppModel
        {
            get
            {
                return _mostUsedAppModel;
            }
            set
            {
                _mostUsedAppModel = value;
                PropertyChanging("MostUsedAppModel");
                LoadSubContent();
            }
        }

        public object SelectedItem
        {
            get;
            set;
        }


        public IEnumerable<MostUsedAppModel> MostUsedAppsList
        {
            get
            {
                return _mostUsedAppsList;
            }
            set
            {
                _mostUsedAppsList = value;
                PropertyChanging("MostUsedAppsList");
            }
        }
        public IEnumerable<DailyAppModel> DailyAppList
        {
            get
            {
                return _dailyAppList;
            }
            set
            {
                _dailyAppList = value;
                PropertyChanging("DailyAppList");
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

        private void ReturnFromDetailedView()
        {
            MostUsedAppModel = null;
        }

        public Statistics_appUsageViewModel()
        {
            Mediator.Register(MediatorMessages.RefreshLogs, new Action(LoadContent));
            _service = ServiceFactory.Get<IChartService>();
        }

        public async void LoadContent()
        {
            await LoadAsync(GetContent, m => MostUsedAppsList = m);
            LoadSubContent();
            IsContentLoaded = true;
        }

        private IEnumerable<MostUsedAppModel> GetContent()
        {
            return _service.GetMostUsedApps(Globals.SelectedUserID, Globals.Date1, Globals.Date2);
        }

        private async void LoadSubContent()
        {
            if (_mostUsedAppModel == null)
                return;

            DailyAppList = null;
            await LoadAsync(GetSubContent, d => DailyAppList = d);
        }

        private IEnumerable<DailyAppModel> GetSubContent()
        {
            var model = _mostUsedAppModel;
            if (model == null)
                return null;

            return _service.GetSingleMostUsedApp(Globals.SelectedUserID, model.AppName, Globals.Date1, Globals.Date2);
        }
    }
}
