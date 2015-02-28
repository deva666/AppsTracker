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
using AppsTracker.Data.Service;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;

namespace AppsTracker.Pages.ViewModels
{
    internal sealed class Statistics_dailyAppUsageViewModel : ViewModelBase, ICommunicator
    {
        #region Fields
        
        private IChartService _chartService;

        private AsyncProperty<IEnumerable<DailyUsedAppsSeries>> _dailyUsedAppsList;

        #endregion

        #region Properties

        public override string Title
        {
            get
            {
                return "DAILY APP USAGE";
            }
        }

        public object SelectedItem
        {
            get;
            set;
        }

        public AsyncProperty<IEnumerable<DailyUsedAppsSeries>> DailyUsedAppsList
        {
            get
            {
                return _dailyUsedAppsList;
            }
        }


        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }

        #endregion

        public Statistics_dailyAppUsageViewModel()
        {            
            _chartService = ServiceFactory.Get<IChartService>();

            _dailyUsedAppsList = new AsyncProperty<IEnumerable<DailyUsedAppsSeries>>(GetContent, this);

            Mediator.Register(MediatorMessages.RefreshLogs, new Action(_dailyUsedAppsList.Reload));
        }

        private IEnumerable<DailyUsedAppsSeries> GetContent()
        {
            return _chartService.GetAppsUsageSeries(Globals.SelectedUserID, Globals.Date1, Globals.Date2);
        }
    }
}
