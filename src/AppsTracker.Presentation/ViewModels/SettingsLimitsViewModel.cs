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
using AppsTracker.Domain.Apps;
using AppsTracker.Domain.Tracking;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class SettingsLimitsViewModel : ViewModelBase
    {
        private const string SETTINGS_SAVED_MSG = "settings saved";

        private readonly AppLimitsCoordinator coordinator;
        private readonly Mediator mediator;

        private readonly ICollection<AppLimitModel> limitsToDelete = new List<AppLimitModel>();


        public override string Title
        {
            get { return "APP LIMITS"; }
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


        private bool isAddNewPopupOpen;

        public bool IsAddNewPopupOpen
        {
            get { return isAddNewPopupOpen; }
            set { SetPropertyValue(ref isAddNewPopupOpen, value); }
        }


        private AppModel selectedApp;

        public AppModel SelectedApp
        {
            get { return selectedApp; }
            set { SetPropertyValue(ref selectedApp, value); }
        }


        private AppLimitModel selectedLimit;

        public AppLimitModel SelectedLimit
        {
            get { return selectedLimit; }
            set { SetPropertyValue(ref selectedLimit, value); }
        }

        public AsyncProperty<IEnumerable<AppModel>> AppList
        {
            get;
            private set;
        }


        private ICommand addNewLimitCommand;

        public ICommand AddNewLimitCommand
        {
            get
            {
                return addNewLimitCommand ??
                    (addNewLimitCommand = new DelegateCommand(AddNewLimit));
            }
        }


        private ICommand saveChangesCommand;

        public ICommand SaveChangesCommand
        {
            get
            {
                return saveChangesCommand ??
                    (saveChangesCommand = new DelegateCommandAsync(SaveChanges));
            }
        }


        private ICommand openAddNewPopupCommand;

        public ICommand OpenAddNewPopupCommand
        {
            get
            {
                return openAddNewPopupCommand ??
                    (openAddNewPopupCommand = new DelegateCommand(OpenAddNewPopup));
            }
        }


        private ICommand deleteSelectedLimitCommand;

        public ICommand DeleteSelectedLimitCommand
        {
            get
            {
                return deleteSelectedLimitCommand ??
                    (deleteSelectedLimitCommand = new DelegateCommand(DeleteSelectedLimit));
            }
        }

        [ImportingConstructor]
        public SettingsLimitsViewModel(AppLimitsCoordinator coordinator,
                                       Mediator mediator)
        {
            this.coordinator = coordinator;
            this.mediator = mediator;

            mediator.Register(MediatorMessages.APPLICATION_ADDED, new Action<Aplication>(OnAppAdded));

            AppList = new TaskRunner<IEnumerable<AppModel>>(coordinator.GetApps, this);
        }

        private void OnAppAdded(Aplication app)
        {
            AppList.Reload();
        }

        private void AddNewLimit(object parameter)
        {
            if (selectedApp == null)
                return;

            string limitSpan = (string)parameter;
            var appLimit = new AppLimitModel() { ApplicationID = selectedApp.ApplicationID };

            if (limitSpan.ToUpper() == "DAILY")
                appLimit.LimitSpan = LimitSpan.Day;
            else if (limitSpan.ToUpper() == "WEEKLY")
                appLimit.LimitSpan = LimitSpan.Week;

            selectedApp.ObservableLimits.Add(appLimit);

            IsAddNewPopupOpen = false;
            PropertyChanging("SelectedApp");
        }


        private void DeleteSelectedLimit()
        {
            if (selectedLimit == null)
                return;

            limitsToDelete.Add(selectedLimit);
            selectedApp.ObservableLimits.Remove(selectedLimit);
        }


        private async Task SaveChanges()
        {
            var apps = AppList.Result;
            if (apps == null)
                return;

            var appsToSave = apps.Where(a => a.Limits.Count() != a.ObservableLimits.Count ||
                a.ObservableLimits.Select(l => l.HasChanges).Any());
            var modifiedLimits = appsToSave.SelectMany(a => a.ObservableLimits)
                .Where(l => l.ID != default(int) && l.HasChanges);
            var newLimits = appsToSave.SelectMany(a => a.ObservableLimits).Where(l => l.ID == default(int));
            await coordinator.SaveChanges(modifiedLimits, newLimits, limitsToDelete);

            limitsToDelete.Clear();

            InfoMessage = SETTINGS_SAVED_MSG;
            mediator.NotifyColleagues(MediatorMessages.APP_LIMITS_CHANGIING);
        }

        private void OpenAddNewPopup()
        {
            IsAddNewPopupOpen = !IsAddNewPopupOpen;
        }
    }
}
