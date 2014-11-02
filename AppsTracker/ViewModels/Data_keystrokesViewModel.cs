using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Data.Entity;
using AppsTracker.Controls;
using AppsTracker.Logging;
using AppsTracker.MVVM;
using AppsTracker.DAL;
using AppsTracker.Models.EntityModels;
using AppsTracker.DAL.Repos;
using AppsTracker.DAL.Service;

namespace AppsTracker.Pages.ViewModels
{
    internal sealed class Data_keystrokesViewModel : ViewModelBase, IChildVM, IWorker, ICommunicator
    {
        #region Fields

        bool _working;

        IEnumerable<Log> _logList;

        IAppsService _service;


        #endregion

        #region Properties

        public string Title
        {
            get
            {
                return "KEYSTROKES";
            }
        }
        public bool IsContentLoaded
        {
            get;
            private set;
        }
        public IEnumerable<Log> LogList
        {
            get
            {
                return _logList;
            }
            set
            {
                _logList = value;
                PropertyChanging("LogList");
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
                _working = value;
                PropertyChanging("Working");
            }
        }

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }

        #endregion

        public Data_keystrokesViewModel()
        {
            Mediator.Register(MediatorMessages.RefreshLogs, new Action(LoadContent));
            _service = ServiceFactory.Get<IAppsService>();
        }

        public async void LoadContent()
        {
            Working = true;
            LogList = await GetContentAsync();
            Working = false;
            IsContentLoaded = true;
        }


        private Task<IEnumerable<Log>> GetContentAsync()
        {
            return _service.GetFilteredAsync<Log>(l => l.KeystrokesRaw != null
                                                        && l.DateCreated >= Globals.Date1
                                                        && l.DateCreated <= Globals.Date2
                                                        && l.Window.Application.UserID == Globals.SelectedUserID
                                                        , l => l.Window.Application
                                                        , l => l.Screenshots);
        }
    }
}
