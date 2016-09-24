#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;
using AppsTracker.Common.Communication;
using AppsTracker.Domain;
using AppsTracker.Domain.Apps;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class AppStatsViewModel : ViewModelBase
    {
        private readonly IUseCase<AppDuration> appDurationUseCase;
        private readonly IUseCase<String, DailyAppDuration> dailyAppDurationUseCase;
        private readonly Mediator mediator;

        public override string Title
        {
            get { return "APPS"; }
        }


        private AppDuration selectedApp;

        public AppDuration SelectedApp
        {
            get { return selectedApp; }
            set
            {
                SetPropertyValue(ref selectedApp, value);
                dailyAppList.Reload();
            }
        }


        public object SelectedItem { get; set; }


        private readonly AsyncProperty<IEnumerable<AppDuration>> appsList;

        public AsyncProperty<IEnumerable<AppDuration>> AppsList
        {
            get { return appsList; }
        }


        private readonly AsyncProperty<IEnumerable<DailyAppDuration>> dailyAppList;

        public AsyncProperty<IEnumerable<DailyAppDuration>> DailyAppList
        {
            get { return dailyAppList; }
        }


        private ICommand returnFromDetailedViewCommand;

        public ICommand ReturnFromDetailedViewCommand
        {
            get { return returnFromDetailedViewCommand ?? (returnFromDetailedViewCommand = new DelegateCommand(ReturnFromDetailedView)); }
        }


        [ImportingConstructor]
        public AppStatsViewModel(IUseCase<AppDuration> appDurationUseCase,
                                 IUseCase<String, DailyAppDuration> dailyAppDurationUseCase,
                                 Mediator mediator)
        {
            this.appDurationUseCase = appDurationUseCase;
            this.dailyAppDurationUseCase = dailyAppDurationUseCase;
            this.mediator = mediator;

            appsList = new TaskRunner<IEnumerable<AppDuration>>(appDurationUseCase.Get, this);
            dailyAppList = new TaskRunner<IEnumerable<DailyAppDuration>>(GetDailyApp, this);

            this.mediator.Register(MediatorMessages.REFRESH_LOGS, new Action(ReloadAll));
        }


        private void ReloadAll()
        {
            appsList.Reload();
            dailyAppList.Reload();
        }

        private IEnumerable<DailyAppDuration> GetDailyApp()
        {
            var app = selectedApp;
            if (app == null)
                return null;

            return dailyAppDurationUseCase.Get(app.Name);
        }


        private void ReturnFromDetailedView()
        {
            SelectedApp = null;
        }
    }
}
