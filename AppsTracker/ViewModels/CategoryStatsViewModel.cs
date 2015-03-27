using System.Collections.Generic;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal sealed class CategoryStatsViewModel : ViewModelBase
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


        public CategoryStatsViewModel()
        {
            statsService = ServiceFactory.Get<IStatsService>();

            categoryList = new AsyncProperty<IEnumerable<CategoryDuration>>(GetCategories, this);
            dailyCategoryList = new AsyncProperty<IEnumerable<DailyCategoryDuration>>(GetDailyCategories, this);
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

        private void ReturnFromDetailedView()
        {
            SelectedCategory = null;
        }
    }
}
