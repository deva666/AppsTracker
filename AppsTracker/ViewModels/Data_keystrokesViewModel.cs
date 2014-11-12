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
    internal sealed class Data_keystrokesViewModel : ViewModelBase, ICommunicator
    {
        #region Fields

        private AsyncProperty<IEnumerable<Log>> _logList;

        private IAppsService _service;


        #endregion

        #region Properties

        public override string Title
        {
            get
            {
                return "KEYSTROKES";
            }
        }

        public AsyncProperty<IEnumerable<Log>> LogList
        {
            get
            {
                return _logList;
            }
        }

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }

        #endregion

        public Data_keystrokesViewModel()
        {
            _service = ServiceFactory.Get<IAppsService>();
            _logList = new AsyncProperty<IEnumerable<Log>>(GetContent, this);

            Mediator.Register(MediatorMessages.RefreshLogs, new Action(_logList.Reload));
        }

        private IEnumerable<Log> GetContent()
        {
            return _service.GetFiltered<Log>(l => l.KeystrokesRaw != null
                                                && l.DateCreated >= Globals.Date1
                                                && l.DateCreated <= Globals.Date2
                                                && l.Window.Application.UserID == Globals.SelectedUserID
                                                , l => l.Window.Application
                                                , l => l.Screenshots);
        }
    }
}
