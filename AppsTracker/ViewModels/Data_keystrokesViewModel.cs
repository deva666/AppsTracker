#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using AppsTracker.DAL.Service;
using AppsTracker.Models.EntityModels;
using AppsTracker.MVVM;

namespace AppsTracker.Pages.ViewModels
{
    internal sealed class Data_keystrokesViewModel : ViewModelBase, ICommunicator
    {
        #region Fields
        
        private IAppsService _appsService;

        private AsyncProperty<IEnumerable<Log>> _logList;

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
            _appsService = ServiceFactory.Get<IAppsService>();

            _logList = new AsyncProperty<IEnumerable<Log>>(GetContent, this);

            Mediator.Register(MediatorMessages.RefreshLogs, new Action(_logList.Reload));
        }

        private IEnumerable<Log> GetContent()
        {
            return _appsService.GetFiltered<Log>(l => l.KeystrokesRaw != null
                                                && l.DateCreated >= Globals.Date1
                                                && l.DateCreated <= Globals.Date2
                                                && l.Window.Application.UserID == Globals.SelectedUserID
                                                , l => l.Window.Application
                                                , l => l.Screenshots);
        }
    }
}
