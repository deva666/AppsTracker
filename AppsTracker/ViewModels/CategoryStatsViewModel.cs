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
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Service;

namespace AppsTracker.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.Any)]
    public sealed class CategoryStatsViewModel : ViewModelBase
    {
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
        public CategoryStatsViewModel(IStatsService statsService,
                                      ITrackingService trackingService,
                                      IMediator mediator)
        {
            this.statsService = statsService;
            this.trackingService = trackingService;
            this.mediator = mediator;

            categoryList = new AsyncProperty<IEnumerable<CategoryDuration>>(GetCategories, this);
            dailyCategoryList = new AsyncProperty<IEnumerable<DailyCategoryDuration>>(GetDailyCategories, this);

            this.mediator.Register(MediatorMessages.REFRESH_LOGS, new Action(ReloadAll));
        }


        private IEnumerable<CategoryDuration> GetCategories()
        {
            return statsService.GetCategoryStats(trackingService.SelectedUserID, trackingService.DateFrom, trackingService.DateTo);
        }


        private IEnumerable<DailyCategoryDuration> GetDailyCategories()
        {
            var category = SelectedCategory;
            if (category == null)
                return null;

            return statsService.GetDailyCategoryStats(trackingService.SelectedUserID, category.Name, trackingService.DateFrom, trackingService.DateTo);
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
