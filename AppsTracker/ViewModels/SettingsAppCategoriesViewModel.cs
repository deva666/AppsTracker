using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal sealed class SettingsAppCategoriesViewModel : ViewModelBase
    {
        private readonly IAppsService appsService;

        public override string Title
        {
            get { return "APP CATEGORIES"; }
        }

        private bool isNewCategoryOpen;
        public bool IsNewCategoryOpen
        {
            get { return isNewCategoryOpen; }
            set
            {
                isNewCategoryOpen = value;
                PropertyChanging("IsNewCategoryOpen");
            }
        }

        private AsyncProperty<ObservableCollection<Aplication>> applications;
        public AsyncProperty<ObservableCollection<Aplication>> Applications
        {
            get { return applications; }
            set
            {
                applications = value;
                PropertyChanging("Applications");
            }
        }

        private ObservableCollection<AppCategory> categories;
        public ObservableCollection<AppCategory> Categories
        {
            get { return categories; }
            set
            {
                categories = value;
                PropertyChanging("Categories");
            }
        }

        private ICommand addNewCategoryCommand;
        public ICommand AddNewCategoryCommand
        {
            get
            {
                return addNewCategoryCommand ?? (addNewCategoryCommand = new DelegateCommand(AddNewCategory));
            }
        }

        private ICommand showNewCategoryCommand;
        public ICommand ShowNewCategoryCommand
        {
            get
            {
                return showNewCategoryCommand ?? (showNewCategoryCommand = new DelegateCommand(ShowNewCategory));
            }
        }

        private void ShowNewCategory(object parameter)
        {
            bool open;
            if (bool.TryParse(parameter as string, out open) == false)
                return;

            IsNewCategoryOpen = open;
        }

        private void AddNewCategory(object parameter)
        {
            var categoryName = parameter as string;

            if (Categories.Any(c => c.Name == categoryName))
                return;

            var category = new AppCategory() { Name = categoryName };
            Categories.Add(category);
            IsNewCategoryOpen = false;
        }

        public SettingsAppCategoriesViewModel()
        {
            appsService = ServiceFactory.Get<IAppsService>();
            Applications = new AsyncProperty<ObservableCollection<Aplication>>(GetApps, this);
            Categories = GetCategories();
        }

        private ObservableCollection<Aplication> GetApps()
        {
            var apps = appsService.Get<Aplication>();
            return new ObservableCollection<Aplication>(apps);
        }

        private ObservableCollection<AppCategory> GetCategories()
        {
            var cats = appsService.Get<AppCategory>(c=>c.Applications);
            return new ObservableCollection<AppCategory>(cats);
        }

    }
}
