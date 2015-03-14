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
using System.Windows.Input;
using AppsTracker.Data.Service;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal sealed class Statistics_keystrokesViewModel : ViewModelBase, ICommunicator
    {
        #region Fields
        
        private IChartService _chartService;

        private ICommand _returnFromDetailedViewCommand;

        private KeystrokeModel _keystrokeModel;

        private AsyncProperty<IEnumerable<KeystrokeModel>> _keystrokeList;

        private AsyncProperty<IEnumerable<DailyKeystrokeModel>> _dailyKeystrokesList;

        #endregion

        #region Properties

        public override string Title
        {
            get
            {
                return "KEYSTROKES";
            }
        }

        public object SelectedItem
        {
            get;
            set;
        }

        public ICommand ReturnFromDetailedViewCommand
        {
            get
            {
                return _returnFromDetailedViewCommand == null ? _returnFromDetailedViewCommand = new DelegateCommand(ReturnFromDetailedView) : _returnFromDetailedViewCommand;
            }
        }

        public KeystrokeModel KeystrokeModel
        {
            get
            {
                return _keystrokeModel;
            }
            set
            {
                _keystrokeModel = value;
                PropertyChanging("KeystrokeModel");
                if (_keystrokeModel != null)
                    _dailyKeystrokesList.Reload();
            }
        }

        public AsyncProperty<IEnumerable<KeystrokeModel>> KeystrokeList
        {
            get
            {
                return _keystrokeList;
            }
        }
        public AsyncProperty<IEnumerable<DailyKeystrokeModel>> DailyKeystrokesList
        {
            get
            {
                return _dailyKeystrokesList;
            }
        }

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }

        #endregion

        public Statistics_keystrokesViewModel()
        {            
            _chartService = ServiceFactory.Get<IChartService>();

            _keystrokeList = new AsyncProperty<IEnumerable<KeystrokeModel>>(GetContent, this);
            _dailyKeystrokesList = new AsyncProperty<IEnumerable<DailyKeystrokeModel>>(GetSubContent, this);

            Mediator.Register(MediatorMessages.RefreshLogs, new Action(ReloadAll));
        }

        private void ReloadAll()
        {
            _keystrokeList.Reload();
            _dailyKeystrokesList.Reload();
        }

        private IEnumerable<KeystrokeModel> GetContent()
        {
            return _chartService.GetKeystrokes(Globals.SelectedUserID, Globals.DateFrom, Globals.DateTo);
        }

        IEnumerable<DailyKeystrokeModel> GetSubContent()
        {
            var model = KeystrokeModel;
            if (model == null)
                return null;

            return _chartService.GetKeystrokesByApp(Globals.SelectedUserID, model.AppName, Globals.DateFrom, Globals.DateTo);
        }

        private void ReturnFromDetailedView()
        {
            KeystrokeModel = null;
        }
    }
}
