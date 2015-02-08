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
using AppsTracker.DAL.Service;
using AppsTracker.Models.ChartModels;
using AppsTracker.MVVM;

namespace AppsTracker.Pages.ViewModels
{
    internal sealed class Statistics_screenshotsViewModel : ViewModelBase, ICommunicator
    {
        #region Fields
        
        private IChartService _chartService;

        private ICommand _returnFromDetailedViewCommand;

        private ScreenshotModel _screenshotModel;

        private AsyncProperty<IEnumerable<ScreenshotModel>> _screenshotList;

        private AsyncProperty<IEnumerable<DailyScreenshotModel>> _dailyScreenshotsList;

        #endregion

        #region Properties

        public override string Title
        {
            get
            {
                return "SCREENSHOTS";
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

        public ScreenshotModel ScreenshotModel
        {
            get
            {
                return _screenshotModel;
            }
            set
            {
                _screenshotModel = value;
                PropertyChanging("ScreenshotModel");
                if (_screenshotModel != null)
                    _dailyScreenshotsList.Reload();
            }
        }

        public AsyncProperty<IEnumerable<ScreenshotModel>> ScreenshotList
        {
            get
            {
                return _screenshotList;
            }
        }

        public AsyncProperty<IEnumerable<DailyScreenshotModel>> DailyScreenshotsList
        {
            get
            {
                return _dailyScreenshotsList;
            }
        }

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }

        #endregion

        public Statistics_screenshotsViewModel()
        {            
            _chartService = ServiceFactory.Get<IChartService>();

            _screenshotList = new AsyncProperty<IEnumerable<ScreenshotModel>>(GetContent, this);
            _dailyScreenshotsList = new AsyncProperty<IEnumerable<DailyScreenshotModel>>(GetSubContent, this);

            Mediator.Register(MediatorMessages.RefreshLogs, new Action(ReloadAll));
        }

        public void ReloadAll()
        {
            _screenshotList.Reload();
            _dailyScreenshotsList.Reload();
        }

        private IEnumerable<ScreenshotModel> GetContent()
        {
            return _chartService.GetScreenshots(Globals.SelectedUserID, Globals.Date1, Globals.Date2);
        }

        private IEnumerable<DailyScreenshotModel> GetSubContent()
        {
            var model = ScreenshotModel;
            if (model == null)
                return null;

            return _chartService.GetScreenshotsByApp(Globals.SelectedUserID, model.AppName, Globals.Date1, Globals.Date2);
        }

        private void ReturnFromDetailedView()
        {
            ScreenshotModel = null;
        }
    }
}
