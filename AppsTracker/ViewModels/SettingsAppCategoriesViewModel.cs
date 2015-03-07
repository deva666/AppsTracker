using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Data.Entity;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal sealed class SettingsAppCategoriesViewModel : ViewModelBase
    {
        private readonly ICategoriesService categoriesService;
        private readonly List<AppCategory> categoriesToDelete = new List<AppCategory>();

        public override string Title
        {
            get { return "APP CATEGORIES"; }
        }

        private bool isNewCategoryOpen;
        public bool IsNewCategoryOpen
        {
            get { return isNewCategoryOpen; }
            set { SetPropertyValue(ref isNewCategoryOpen, value); }
        }

        private string newCategoryName;
        public string NewCategoryName
        {
            get { return newCategoryName; }
            set { SetPropertyValue(ref newCategoryName, value); }
        }

        private ObservableCollection<Aplication> applications;
        public ObservableCollection<Aplication> Applications
        {
            get { return applications; }
            set { SetPropertyValue(ref applications, value); }
        }

        private ObservableCollection<AppCategory> categories;
        public ObservableCollection<AppCategory> Categories
        {
            get { return categories; }
            set { SetPropertyValue(ref categories, value); }
        }

        public Aplication UnassignedSelectedApp { get; set; }
        public Aplication AssignedSelectedApp { get; set; }
        public AppCategory SelectedCategory { get; set; }

        private ICommand addNewCategoryCommand;
        public ICommand AddNewCategoryCommand
        {
            get { return addNewCategoryCommand ?? (addNewCategoryCommand = new DelegateCommand(AddNewCategory)); }
        }

        private ICommand showNewCategoryCommand;
        public ICommand ShowNewCategoryCommand
        {
            get { return showNewCategoryCommand ?? (showNewCategoryCommand = new DelegateCommand(ShowNewCategory)); }
        }

        private ICommand saveChangesCommand;
        public ICommand SaveChangesCommand
        {
            get { return saveChangesCommand ?? (saveChangesCommand = new DelegateCommand(SaveChanges)); }
        }

        private ICommand assignAppCommand;
        public ICommand AssignAppCommand
        {
            get { return assignAppCommand ?? (assignAppCommand = new DelegateCommand(AssignApp)); }
        }

        private ICommand removeAppCommand;
        public ICommand RemoveAppCommand
        {
            get { return removeAppCommand ?? (removeAppCommand = new DelegateCommand(RemoveApp)); }
        }

        private ICommand deleteCategoryCommand;
        public ICommand DeleteCategoryCommand
        {
            get { return deleteCategoryCommand ?? (deleteCategoryCommand = new DelegateCommand(DeleteCategory)); }
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

            var category = new AppCategory() { Name = categoryName, ObservableApplications = new ObservableCollection<Aplication>() };
            Categories.Add(category);
            IsNewCategoryOpen = false;
            NewCategoryName = null;
        }

        public SettingsAppCategoriesViewModel()
        {
            categoriesService = ServiceFactory.Get<ICategoriesService>();
            LoadContent();
            Mediator.Instance.Register<Aplication>(MediatorMessages.ApplicationAdded, AppAdded);
        }

        private void LoadContent()
        {
            Categories = GetCategories();
            var unassignedApps = GetApps();
            var assignedApps = Categories.SelectMany(c => c.Applications);
            unassignedApps.RemoveAll(a => assignedApps.Any(app => app.ApplicationID == a.ApplicationID));
            Applications = new ObservableCollection<Aplication>(unassignedApps);
        }

        private List<Aplication> GetApps()
        {
            return categoriesService.GetApps();
        }

        private ObservableCollection<AppCategory> GetCategories()
        {
            return categoriesService.GetCategories();
        }

        private void AssignApp()
        {
            if (UnassignedSelectedApp == null || SelectedCategory == null)
                return;

            SelectedCategory.ObservableApplications.Add(UnassignedSelectedApp);
            Applications.Remove(UnassignedSelectedApp);
            PropertyChanging("Categories.Applications");
        }

        private void RemoveApp()
        {
            if (AssignedSelectedApp == null || SelectedCategory == null)
                return;
            Applications.Add(AssignedSelectedApp);
            SelectedCategory.ObservableApplications.Remove(AssignedSelectedApp);
        }

        private void DeleteCategory()
        {
            if (SelectedCategory == null)
                return;
            categoriesToDelete.Add(SelectedCategory);
            Categories.Remove(SelectedCategory);
        }

        private void AppAdded(Aplication app)
        {
            Applications.Add(app);
        }

        private void SaveChanges()
        {
            categoriesService.SaveChanges(categoriesToDelete, Categories);
        }

        private void SetApplications(AppCategory cat)
        {
            cat.Applications.Clear();
            foreach (var app in cat.ObservableApplications)
            {
                cat.Applications.Add(app);
            }
        }

        //Finalizer is used instead of Dispose becouse Host View Models store all references to child view models as weak refrences and we don't know when the GC is going to kick in and free the resurce 
        ~SettingsAppCategoriesViewModel()
        {
            categoriesService.Dispose();
        }

    }
}
