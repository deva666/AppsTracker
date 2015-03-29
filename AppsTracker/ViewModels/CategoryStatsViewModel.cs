#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal sealed class CategoryStatsViewModel : ViewModelBase, ICommunicator
    {
        private readonly IStatsService statsService;


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

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }


        public CategoryStatsViewModel()
        {
            statsService = serviceResolver.Resolve<IStatsService>();

            categoryList = new AsyncProperty<IEnumerable<CategoryDuration>>(GetCategories, this);
            dailyCategoryList = new AsyncProperty<IEnumerable<DailyCategoryDuration>>(GetDailyCategories, this);
            Mediator.Register(MediatorMessages.RefreshLogs, new Action(ReloadAll));
        }


        private IEnumerable<CategoryDuration> GetCategories()
        {
            return statsService.GetCategoryStats(Globals.SelectedUserID, Globals.DateFrom, Globals.DateTo);
        }


        private IEnumerable<DailyCategoryDuration> GetDailyCategories()
        {
            var category = SelectedCategory;
            if (category == null)
                return null;

            return statsService.GetDailyCategoryStats(Globals.SelectedUserID, category.Name, Globals.DateFrom, Globals.DateTo);
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
