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
        //private readonly IAppsService appsService;
        private readonly AppsTracker.Data.Db.AppsEntities _context;
        private readonly List<AppCategory> _categoriesToDelete = new List<AppCategory>();

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

        private ObservableCollection<Aplication> applications;
        public ObservableCollection<Aplication> Applications
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

        public Aplication UnassignedSelectedApp { get; set; }
        public Aplication AssignedSelectedApp { get; set; }
        public AppCategory SelectedCategory { get; set; }

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

        private ICommand saveChangesCommand;
        public ICommand SaveChangesCommand
        {
            get
            {
                return saveChangesCommand ?? (saveChangesCommand = new DelegateCommand(SaveChanges));
            }
        }

        private ICommand assignAppCommand;
        public ICommand AssignAppCommand
        {
            get
            {
                return assignAppCommand ?? (assignAppCommand = new DelegateCommand(AssignApp));
            }
        }

        private ICommand removeAppCommand;
        public ICommand RemoveAppCommand
        {
            get
            {
                return removeAppCommand ?? (removeAppCommand = new DelegateCommand(RemoveApp));
            }
        }

        private ICommand deleteCategoryCommand;
        public ICommand DeleteCategoryCommand
        {
            get
            {
                return deleteCategoryCommand ?? (deleteCategoryCommand = new DelegateCommand(DeleteCategory));
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

            var category = new AppCategory() { Name = categoryName, ObservableApplications = new ObservableCollection<Aplication>() };
            Categories.Add(category);
            IsNewCategoryOpen = false;
        }

        public SettingsAppCategoriesViewModel()
        {
            //appsService = ServiceFactory.Get<IAppsService>();
            _context = new Data.Db.AppsEntities();
            LoadContent();
            Mediator.Instance.Register<Aplication>(MediatorMessages.ApplicationAdded, AppAdded);
        }

        private void LoadContent()
        {
            Categories = GetCategories();
            var allApps = GetApps();
            var assignedApps = Categories.SelectMany(c => c.Applications);
            allApps.RemoveAll(a => assignedApps.Any(app => app.ApplicationID == a.ApplicationID));
            Applications = new ObservableCollection<Aplication>(allApps);
        }

        private List<Aplication> GetApps()
        {
            var apps = _context.Applications;
            return apps.ToList();
        }

        private ObservableCollection<AppCategory> GetCategories()
        {
            var categories = _context.AppCategories.Include(c => c.Applications);
            foreach (var cat in categories)
            {
                cat.ObservableApplications = new ObservableCollection<Aplication>(cat.Applications);
            }
            return new ObservableCollection<AppCategory>(categories);
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
            _categoriesToDelete.Add(SelectedCategory);
            Categories.Remove(SelectedCategory);
        }

        private void AppAdded(Aplication app)
        {
            Applications.Add(app);
        }

        private void SaveChanges()
        {
            foreach (var cat in _categoriesToDelete)
            {
                _context.Entry(cat).State = EntityState.Deleted;    
            }

            foreach (var cat in Categories)
            {
                if (cat.AppCategoryID == default(int))
                    _context.Entry(cat).State = System.Data.Entity.EntityState.Added;
                else
                    _context.Entry(cat).State = System.Data.Entity.EntityState.Modified;

                SetApplications(cat);
            }

            _context.SaveChanges();
        }

        private void SetApplications(AppCategory cat)
        {
            cat.Applications.Clear();
            foreach (var app in cat.ObservableApplications)
            {
                cat.Applications.Add(app);
            }
        }

        protected override void Disposing()
        {
            _context.Dispose();
            Console.WriteLine("DISPOSED CONTEXT");
            base.Disposing();
        }
    }
}
