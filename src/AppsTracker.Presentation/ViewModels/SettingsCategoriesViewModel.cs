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
using AppsTracker.Domain.Apps;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class SettingsAppCategoriesViewModel : ViewModelBase
    {
        private const string SETTINGS_SAVED_MSG = "settings saved";

        private readonly AppCategoriesCoordinator coordinator;
        private readonly Mediator mediator;

        private readonly ICollection<AppCategoryModel> categoriesToDelete = new List<AppCategoryModel>();


        public override string Title
        {
            get { return "APP CATEGORIES"; }
        }


        private string infoMessage;

        public string InfoMessage
        {
            get { return infoMessage; }
            set
            {
                SetPropertyValue(ref infoMessage, string.Empty);
                SetPropertyValue(ref infoMessage, value);
            }
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


        private ObservableCollection<AppModel> applications;

        public ObservableCollection<AppModel> Applications
        {
            get { return applications; }
            set { SetPropertyValue(ref applications, value); }
        }


        private ObservableCollection<AppCategoryModel> categories;

        public ObservableCollection<AppCategoryModel> Categories
        {
            get { return categories; }
            set { SetPropertyValue(ref categories, value); }
        }


        public AppModel UnassignedSelectedApp { get; set; }


        public AppModel AssignedSelectedApp { get; set; }


        public AppCategoryModel SelectedCategory { get; set; }


        private ICommand addNewCategoryCommand;

        public ICommand AddNewCategoryCommand
        {
            get
            {
                return addNewCategoryCommand
                  ?? (addNewCategoryCommand = new DelegateCommand(AddNewCategory));
            }
        }


        private ICommand showNewCategoryCommand;

        public ICommand ShowNewCategoryCommand
        {
            get
            {
                return showNewCategoryCommand
                  ?? (showNewCategoryCommand = new DelegateCommand(ShowNewCategory));
            }
        }


        private ICommand saveChangesCommand;

        public ICommand SaveChangesCommand
        {
            get
            {
                return saveChangesCommand
                  ?? (saveChangesCommand = new DelegateCommandAsync(SaveChanges));
            }
        }


        private ICommand assignAppCommand;

        public ICommand AssignAppCommand
        {
            get
            {
                return assignAppCommand
                  ?? (assignAppCommand = new DelegateCommand(AssignApp));
            }
        }


        private ICommand removeAppCommand;

        public ICommand RemoveAppCommand
        {
            get
            {
                return removeAppCommand
                  ?? (removeAppCommand = new DelegateCommand(RemoveApp));
            }
        }


        private ICommand deleteCategoryCommand;

        public ICommand DeleteCategoryCommand
        {
            get
            {
                return deleteCategoryCommand
                  ?? (deleteCategoryCommand = new DelegateCommand(DeleteCategory));
            }
        }


        [ImportingConstructor]
        public SettingsAppCategoriesViewModel(AppCategoriesCoordinator coordinator,
                                              Mediator mediator)
        {
            this.coordinator = coordinator;
            this.mediator = mediator;

            Task.Run(new Action(LoadContent));
            this.mediator.Register<Aplication>(MediatorMessages.APPLICATION_ADDED, AppAdded);
        }


        private void LoadContent()
        {
            var apps = coordinator.GetApps().ToList();
            Categories = new ObservableCollection<AppCategoryModel>(coordinator.GetCategories());
            var assignedApps = Categories.SelectMany(c => c.Applications);
            apps.RemoveAll(a => assignedApps.Any(app => app.ApplicationID == a.ApplicationID));
            Applications = new ObservableCollection<AppModel>(apps);
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
            var categoryName = (string)parameter;

            if (Categories.Any(c => c.Name == categoryName))
                return;

            var category = new AppCategoryModel(categoryName);
            Categories.Add(category);
            IsNewCategoryOpen = false;
            NewCategoryName = null;
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

            if (SelectedCategory.ID != default(int))
                categoriesToDelete.Add(SelectedCategory);

            Categories.Remove(SelectedCategory);
        }


        private void AppAdded(Aplication app)
        {
            Applications.Add(new AppModel(app));
        }


        private async Task SaveChanges()
        {
            var newCategories = Categories.Where(c => c.ID == default(int));
            var modifiedCategories = Categories.Where(c => c.ID != default(int));
            await coordinator.SaveChangesAsync(categoriesToDelete, newCategories, modifiedCategories);
            categoriesToDelete.Clear();
            InfoMessage = SETTINGS_SAVED_MSG;
        }
    }
}
