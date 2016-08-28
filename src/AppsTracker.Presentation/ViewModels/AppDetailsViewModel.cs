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
using System.Linq;
using System.Windows.Input;
using AppsTracker.Common.Communication;
using AppsTracker.Data.Models;
using AppsTracker.Domain;
using AppsTracker.Domain.Apps;
using AppsTracker.Domain.Windows;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class AppDetailsViewModel : ViewModelBase
    {
        private readonly IUseCaseAsync<AppModel> appUseCase;
        private readonly IUseCase<String, Int32, AppSummary> appSummaryUseCase;
        private readonly IUseCase<String, IEnumerable<DateTime>, WindowSummary> windowSummaryUseCase;
        private readonly IUseCase<String, IEnumerable<String>, IEnumerable<DateTime>, WindowDurationOverview> windowDurationUseCase;
        private readonly IMediator mediator;

        public override string Title
        {
            get { return "APPS"; }
        }


        private bool isChartVisible;

        public bool IsChartVisible
        {
            get { return isChartVisible; }
            set { SetPropertyValue(ref isChartVisible, value); }
        }


        private string selectedAppsDuration;

        public string SelectedAppsDuration
        {
            get { return selectedAppsDuration; }
            set { SetPropertyValue(ref selectedAppsDuration, value); }
        }


        private string selectedWindowsDuration;

        public string SelectedWindowsDuration
        {
            get { return selectedWindowsDuration; }
            set { SetPropertyValue(ref selectedWindowsDuration, value); }
        }


        private readonly AsyncProperty<IEnumerable<AppModel>> appList;

        public AsyncProperty<IEnumerable<AppModel>> AppList
        {
            get { return appList; }
        }


        private readonly AsyncProperty<IEnumerable<AppSummary>> appSummaryList;

        public AsyncProperty<IEnumerable<AppSummary>> AppSummaryList
        {
            get { return appSummaryList; }
        }


        private readonly AsyncProperty<IEnumerable<WindowSummary>> windowSummaryList;

        public AsyncProperty<IEnumerable<WindowSummary>> WindowSummaryList
        {
            get { return windowSummaryList; }
        }


        private readonly AsyncProperty<IEnumerable<WindowDurationOverview>> windowDurationList;

        public AsyncProperty<IEnumerable<WindowDurationOverview>> WindowDurationList
        {
            get { return windowDurationList; }
        }


        private AppSummary selectedAppSummary;

        public AppSummary SelectedAppSummary
        {
            get { return selectedAppSummary; }
            set
            {
                SetPropertyValue(ref selectedAppSummary, value);
                SelectedWindowsDuration = string.Empty;
            }
        }


        private AppModel selectedApp;

        public AppModel SelectedApp
        {
            get { return selectedApp; }
            set
            {
                SetPropertyValue(ref selectedApp, value);
                IsChartVisible = false;
                if (value != null)
                {
                    appSummaryList.Reload();
                }
            }
        }


        private ICommand selectedAppsChangingCommand;

        public ICommand SelectedAppsChangingCommand
        {
            get { return selectedAppsChangingCommand ?? (selectedAppsChangingCommand = new DelegateCommand(SelectedAppChanged)); }
        }


        private ICommand selectedWindowsChangingCommand;

        public ICommand SelectedWindowsChangingCommand
        {
            get { return selectedWindowsChangingCommand ?? (selectedWindowsChangingCommand = new DelegateCommand(SelectedWindowChanged)); }
        }


        [ImportingConstructor]
        public AppDetailsViewModel(IUseCaseAsync<AppModel> appUseCase,
                                   IUseCase<String, Int32, AppSummary> appSummaryUseCase,
                                   IUseCase<String, IEnumerable<DateTime>, WindowSummary> windowSummaryUseCase,
                                   IUseCase<String, IEnumerable<String>, IEnumerable<DateTime>, WindowDurationOverview> windowDurationUseCase,
                                   IMediator mediator)
        {
            this.appUseCase = appUseCase;
            this.appSummaryUseCase = appSummaryUseCase;
            this.windowSummaryUseCase = windowSummaryUseCase;
            this.windowDurationUseCase = windowDurationUseCase;
            this.mediator = mediator;

            appList = new TaskObserver<IEnumerable<AppModel>>(appUseCase.GetAsync, this);
            appSummaryList = new TaskRunner<IEnumerable<AppSummary>>(GetAppSummary, this);
            windowSummaryList = new TaskRunner<IEnumerable<WindowSummary>>(GetWindowSummary, this);
            windowDurationList = new TaskRunner<IEnumerable<WindowDurationOverview>>(GetWindowDuration, this);

            this.mediator.Register(MediatorMessages.APPLICATION_ADDED, new Action<Aplication>(ApplicationAdded));
            this.mediator.Register(MediatorMessages.REFRESH_LOGS, new Action(ReloadAll));
        }

        private IEnumerable<AppSummary> GetAppSummary()
        {
            var app = SelectedApp;
            if (app == null)
                return null;

            return appSummaryUseCase.Get(app.Name, app.ApplicationID);
        }

        private IEnumerable<WindowSummary> GetWindowSummary()
        {
            var selectedApp = SelectedAppSummary;
            if (AppSummaryList.Result == null || selectedApp == null)
                return null;

            var selectedDates = AppSummaryList.Result.Where(t => t.IsSelected).Select(t => t.DateTime);

            return windowSummaryUseCase.Get(selectedApp.AppName, selectedDates);
        }


        private IEnumerable<WindowDurationOverview> GetWindowDuration()
        {
            var selectedApp = SelectedAppSummary;

            if (AppSummaryList.Result == null || selectedApp == null || WindowSummaryList.Result == null)
                return null;

            var selectedWindows = WindowSummaryList.Result.Where(w => w.IsSelected).Select(w => w.Title).ToList();
            if (selectedWindows.Count() == 0)
                return null;

            var selectedDates = AppSummaryList.Result.Where(t => t.IsSelected).Select(t => t.DateTime);

            return windowDurationUseCase.Get(selectedApp.AppName, selectedWindows, selectedDates);
        }


        private void ApplicationAdded(Aplication app)
        {
            ReloadAll();
        }

        private void ReloadAll()
        {
            SelectedApp = null;
            SelectedAppSummary = null;

            appList.Reload();

            windowDurationList.Reset();
            appSummaryList.Reset();
            windowSummaryList.Reset();
        }

        private void SelectedAppChanged()
        {
            IsChartVisible = false;

            if (AppSummaryList.Result == null)
                return;

            long ticks = 0;
            var selectedApps = AppSummaryList.Result.Where(a => a.IsSelected);

            if (selectedApps.Count() == 0)
            {
                windowSummaryList.Reset();
                return;
            }

            foreach (var app in selectedApps)
                ticks += app.Duration;

            if (ticks == 0)
                return;

            var timeSpan = new TimeSpan(ticks);
            SelectedAppsDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            windowSummaryList.Reload();
        }


        private void SelectedWindowChanged()
        {
            if (windowSummaryList.Result == null)
            {
                WindowDurationList.Reset();
                IsChartVisible = false;
                return;
            }

            if (windowSummaryList.Result.Where(w => w.IsSelected).Count() == 0)
            {
                IsChartVisible = false;
                return;
            }

            IsChartVisible = true;
            long ticks = 0;

            foreach (var window in windowSummaryList.Result.Where(w => w.IsSelected))
                ticks += window.Duration;

            if (ticks == 0)
                return;

            var timeSpan = new TimeSpan(ticks);
            SelectedWindowsDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

            windowDurationList.Reload();
        }
    }
}
