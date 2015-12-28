#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Data.Service;
using AppsTracker.Common.Communication;
using AppsTracker.Tracking;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class CategoryStatsViewModel : ViewModelBase
    {
        private readonly IDataService dataService;
        private readonly IStatsService statsService;
        private readonly ITrackingService trackingService;
        private readonly IMediator mediator;

        public override string Title
        {
            get { return "CATEGORIES"; }
        }


        private CategoryDuration selectedCategory;

        public CategoryDuration SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                SetPropertyValue(ref selectedCategory, value);
                if (selectedCategory != null)
                    dailyCategoryList.Reload();
            }
        }


        private readonly AsyncProperty<IEnumerable<CategoryDuration>> categoryList;

        public AsyncProperty<IEnumerable<CategoryDuration>> CategoryList
        {
            get { return categoryList; }
        }


        private readonly AsyncProperty<IEnumerable<DailyCategoryDuration>> dailyCategoryList;

        public AsyncProperty<IEnumerable<DailyCategoryDuration>> DailyCategoryList
        {
            get { return dailyCategoryList; }
        }


        private ICommand returnFromDetailedViewCommand;

        public ICommand ReturnFromDetailedViewCommand
        {
            get { return returnFromDetailedViewCommand ?? (returnFromDetailedViewCommand = new DelegateCommand(ReturnFromDetailedView)); }
        }


        [ImportingConstructor]
        public CategoryStatsViewModel(IDataService dataService,
                                      IStatsService statsService,
                                      ITrackingService trackingService,
                                      IMediator mediator)
        {
            this.dataService = dataService;
            this.statsService = statsService;
            this.trackingService = trackingService;
            this.mediator = mediator;

            categoryList = new TaskRunner<IEnumerable<CategoryDuration>>(GetCategories, this);
            dailyCategoryList = new TaskRunner<IEnumerable<DailyCategoryDuration>>(GetDailyCategories, this);

            this.mediator.Register(MediatorMessages.REFRESH_LOGS, new Action(ReloadAll));
        }


        private IEnumerable<CategoryDuration> GetCategories()
        {
            var dateTo = trackingService.DateFrom.AddDays(1);
            var categoryModels = new List<CategoryDuration>();

            var categories = dataService.GetFiltered<AppCategory>(c => c.Applications.Count > 0 
                        && c.Applications
                        .Where(a => a.UserID == trackingService.SelectedUserID)
                        .Any() 
                        && c.Applications
                        .SelectMany(a => a.Windows)
                        .SelectMany(w => w.Logs)
                        .Where(l => l.DateCreated >= trackingService.DateFrom)
                        .Any() 
                        && c.Applications
                        .SelectMany(a => a.Windows)
                        .SelectMany(w => w.Logs)
                        .Where(l => l.DateCreated <= dateTo)
                        .Any(),
                       c => c.Applications);
            
            foreach (var cat in categories)
            {
                var totalDuration = cat.Applications
                                       .SelectMany(a => a.Windows)
                                       .SelectMany(w => w.Logs)
                                       .Where(l => l.DateCreated >= trackingService.DateFrom 
                                           && l.DateCreated <= dateTo)
                                       .Sum(l => l.Duration);

                categoryModels.Add(new CategoryDuration()
                {
                    Name = cat.Name,
                    TotalTime = Math.Round(new TimeSpan(totalDuration).TotalHours, 2)
                });
            }

            return categoryModels;            
        }


        private IEnumerable<DailyCategoryDuration> GetDailyCategories()
        {
            var category = SelectedCategory;
            if (category == null)
                return null;

            var logs = dataService.GetFiltered<Log>(l => l.Window.Application.Categories.Any(c => c.Name == category.Name)
                                               && l.Window.Application.UserID == trackingService.SelectedUserID
                                               && l.DateCreated >= trackingService.DateFrom
                                               && l.DateCreated <= trackingService.DateTo);                                       

            var grouped = logs.GroupBy(l => new
            {
                year = l.DateCreated.Year,
                month = l.DateCreated.Month,
                day = l.DateCreated.Day
            });

            return grouped.Select(g => new DailyCategoryDuration()
            {
                Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString(),
                TotalTime = Math.Round(new TimeSpan(g.Sum(l => l.Duration)).TotalHours, 2)
            });            
        }

        private void ReloadAll()
        {
            categoryList.Reload();
            dailyCategoryList.Reload();
        }


        private void ReturnFromDetailedView()
        {
            SelectedCategory = null;
        }
    }
}
