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
    public sealed class CategoryStatsViewModel : ViewModelBase
    {
        private readonly IUseCase<CategoryDuration> categoryDurationUseCase;
        private readonly IUseCase<String, DailyCategoryDuration> dailyCategoryDurationUseCase;
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
        public CategoryStatsViewModel(IUseCase<CategoryDuration> categoryDurationUseCase,
                                      IUseCase<String, DailyCategoryDuration> dailyCategoryDurationUseCase,
                                      IMediator mediator)
        {
            this.categoryDurationUseCase = categoryDurationUseCase;
            this.dailyCategoryDurationUseCase = dailyCategoryDurationUseCase;
            this.mediator = mediator;

            categoryList = new TaskRunner<IEnumerable<CategoryDuration>>(categoryDurationUseCase.Get, this);
            dailyCategoryList = new TaskRunner<IEnumerable<DailyCategoryDuration>>(GetDailyCategories, this);

            this.mediator.Register(MediatorMessages.REFRESH_LOGS, new Action(ReloadAll));
        }


        private IEnumerable<DailyCategoryDuration> GetDailyCategories()
        {
            var category = SelectedCategory;
            if (category == null)
                return null;

            return dailyCategoryDurationUseCase.Get(category.Name);
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
