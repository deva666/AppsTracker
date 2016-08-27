#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.Common.Communication;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Data.Utils;
using AppsTracker.Domain.UseCases;
using AppsTracker.MVVM;
using AppsTracker.Tracking;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class DaySummaryViewModel : ViewModelBase
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;
        private readonly IUseCase<DateTime, LogSummary> logSummaryUseCase;
        private readonly IUseCase<DateTime, AppSummary> appSummaryUseCase;
        private readonly IUseCase<String, DateTime, WindowSummary> windowSummaryUseCase;
        private readonly IUseCase<DateTime, UsageByTime> usageByTimeUseCase;
        private readonly IMediator mediator;


        public override string Title
        {
            get { return "DAY SUMMARY"; }
        }


        public string DayOfWeek
        {
            get { return selectedDate.DayOfWeek.ToString(); }
        }


        private DateTime selectedDate = DateTime.Today;

        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set
            {
                selectedDate = value;
                PropertyChanging("SelectedDate");
                PropertyChanging("DayOfWeek");
                ReloadContent();
            }
        }


        private string selectedWindowsDuration;

        public string SelectedWindowsDuration
        {
            get { return selectedWindowsDuration; }
            set { SetPropertyValue(ref selectedWindowsDuration, value); }
        }


        private AppSummary selectedApp;

        public AppSummary SelectedApp
        {
            get { return selectedApp; }
            set
            {
                SetPropertyValue(ref selectedApp, value);
                SelectedWindowsDuration = string.Empty;
                if (value != null)
                    windowsList.Reload();
            }
        }


        private readonly AsyncProperty<IEnumerable<AppSummary>> appsList;

        public AsyncProperty<IEnumerable<AppSummary>> AppsList
        {
            get { return appsList; }
        }


        private readonly AsyncProperty<IEnumerable<LogSummary>> logsList;

        public AsyncProperty<IEnumerable<LogSummary>> LogsList
        {
            get { return logsList; }
        }


        private readonly AsyncProperty<IEnumerable<WindowSummary>> windowsList;

        public AsyncProperty<IEnumerable<WindowSummary>> WindowsList
        {
            get { return windowsList; }
        }


        private readonly AsyncProperty<IEnumerable<UsageByTime>> usageList;

        public AsyncProperty<IEnumerable<UsageByTime>> UsageList
        {
            get { return usageList; }
        }


        private readonly AsyncProperty<IEnumerable<CategoryDuration>> categoryList;

        public AsyncProperty<IEnumerable<CategoryDuration>> CategoryList
        {
            get { return categoryList; }
        }


        private ICommand selectedWindowsChangingCommand;

        public ICommand SelectedWindowsChangingCommand
        {
            get { return selectedWindowsChangingCommand ?? (selectedWindowsChangingCommand = new DelegateCommand(SelectedWindowsChanging)); }
        }


        private ICommand changeDateCommand;

        public ICommand ChangeDateCommand
        {
            get { return changeDateCommand ?? (changeDateCommand = new DelegateCommand(ChangeDate)); }
        }



        [ImportingConstructor]
        public DaySummaryViewModel(IRepository repository,
                                   ITrackingService trackingService,
                                   IUseCase<DateTime, LogSummary> logSummaryUseCase,
                                   IUseCase<DateTime, AppSummary> appSummaryUseCase,
                                   IUseCase<String, DateTime, WindowSummary> windowSummaryUseCase,
                                   IUseCase<DateTime, UsageByTime> usageByTimeUseCase,
                                   IMediator mediator)
        {
            this.repository = repository;
            this.trackingService = trackingService;
            this.logSummaryUseCase = logSummaryUseCase;
            this.appSummaryUseCase = appSummaryUseCase;
            this.windowSummaryUseCase = windowSummaryUseCase;
            this.usageByTimeUseCase = usageByTimeUseCase;
            this.mediator = mediator;

            logsList = new TaskRunner<IEnumerable<LogSummary>>(GetLogSummary, this);
            appsList = new TaskRunner<IEnumerable<AppSummary>>(GetAppsSummary, this);
            usageList = new TaskRunner<IEnumerable<UsageByTime>>(GetUsageSummary, this);
            windowsList = new TaskRunner<IEnumerable<WindowSummary>>(GetWindowsSummary, this);
            categoryList = new TaskRunner<IEnumerable<CategoryDuration>>(GetCategories, this);

            this.mediator.Register(MediatorMessages.REFRESH_LOGS, new Action(ReloadContent));
        }


        private void ReloadContent()
        {
            logsList.Reload();
            appsList.Reload();
            usageList.Reload();
            windowsList.Reload();
            categoryList.Reload();
        }


        private IEnumerable<LogSummary> GetLogSummary()
        {
            return logSummaryUseCase.Get(selectedDate);
        }


        private IEnumerable<AppSummary> GetAppsSummary()
        {
            return appSummaryUseCase.Get(selectedDate);
        }


        private IEnumerable<WindowSummary> GetWindowsSummary()
        {
            var model = selectedApp;
            if (model == null)
                return null;

            return windowSummaryUseCase.Get(model.AppName, selectedDate);
        }


        private IEnumerable<UsageByTime> GetUsageSummary()
        {
            return usageByTimeUseCase.Get(selectedDate);
        }


        private IEnumerable<CategoryDuration> GetCategories()
        {
            var dateTo = selectedDate.AddDays(1);

            var categories = repository.GetFiltered<AppCategory>(c => c.Applications.Count > 0 &&
                       c.Applications.Where(a => a.UserID == trackingService.SelectedUserID).Any() &&
                       c.Applications.SelectMany(a => a.Windows).SelectMany(w => w.Logs).Where(l => l.DateCreated >= selectedDate).Any() &&
                       c.Applications.SelectMany(a => a.Windows).SelectMany(w => w.Logs).Where(l => l.DateCreated <= dateTo).Any(),
                      c => c.Applications.Select(a => a.Windows.Select(w => w.Logs)),
                      c => c.Applications);

            var categoryModels = new List<CategoryDuration>();
            foreach (var cat in categories)
            {
                var totalDuration = cat.Applications
                    .SelectMany(a => a.Windows)
                    .SelectMany(w => w.Logs)
                    .Where(l => l.DateCreated >= selectedDate && l.DateCreated <= dateTo)
                    .Sum(l => l.Duration);

                categoryModels.Add(new CategoryDuration()
                {
                    Name = cat.Name,
                    TotalTime = Math.Round(new TimeSpan(totalDuration).TotalHours, 2)
                });
            }

            return categoryModels;
        }


        private void SelectedWindowsChanging()
        {
            var topWindows = windowsList.Result;
            if (topWindows == null)
                return;

            long selectedWindowsDuration = 0;
            selectedWindowsDuration = topWindows.Where(t => t.IsSelected)
                                                .Sum(w => w.Duration);
            
            if (selectedWindowsDuration == 0)
                return;

            var timeSpan = new TimeSpan(selectedWindowsDuration);
            SelectedWindowsDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }


        private void ChangeDate(object parameter)
        {
            string stringParameter = parameter as string;
            if (stringParameter == null)
                return;
            switch (stringParameter)
            {
                case "+":
                    SelectedDate = SelectedDate.AddDays(1d);
                    break;
                case "-":
                    SelectedDate = SelectedDate.AddDays(-1d);
                    break;
                default:
                    SelectedDate = DateTime.Today;
                    break;
            }
        }
    }
}
