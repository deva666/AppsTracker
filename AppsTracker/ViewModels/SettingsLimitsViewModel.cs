using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Service;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class SettingsLimitsViewModel : ViewModelBase
    {
        private const string SETTINGS_SAVED_MSG = "settings saved";

        private readonly IDataService dataService;
        private readonly ITrackingService trackingService;


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


        private Aplication selectedApp;

        public Aplication SelectedApp
        {
            get { return selectedApp; }
            set { SetPropertyValue(ref selectedApp, value); }
        }

        public AsyncProperty<IEnumerable<Aplication>> AppList
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


        [ImportingConstructor]
        public SettingsLimitsViewModel(IDataService dataService,
                                       ITrackingService trackingService)
        {
            this.dataService = dataService;
            this.trackingService = trackingService;

            AppList = new AsyncProperty<IEnumerable<Aplication>>(GetApps, this);
        }

        private IEnumerable<Aplication> GetApps()
        {
            var apps = dataService.GetFiltered<Aplication>(a => a.User.UserID == trackingService.SelectedUserID,
                                                           a => a.Limits)
                                                        .ToList()
                                                        .Distinct();
            foreach (var app in apps)
            {
                app.ObservableLimits = new System.Collections.ObjectModel.ObservableCollection<AppLimit>(app.Limits);
            }

            return apps;
        }


        private void AddNewLimit(object parameter)
        {
            if (selectedApp == null)
                return;

            string limit = (string)parameter;
            var appLimit = new AppLimit() { ApplicationID = selectedApp.ApplicationID };

            if (limit.ToUpper() == "DAILY")
                appLimit.LimitSpan = LimitSpan.Day;
            else if (limit.ToUpper() == "WEEKLY")
                appLimit.LimitSpan = LimitSpan.Week;
            else if (limit.ToUpper() == "MONTHLY")
                appLimit.LimitSpan = LimitSpan.Month;

            selectedApp.ObservableLimits.Add(appLimit);

            IsAddNewPopupOpen = false;
            PropertyChanging("SelectedApp");
        }

        private async Task SaveChanges()
        {
            var apps = AppList.Result;
            if (apps == null)
                return;

            var appsToSave = apps.Where(a => a.Limits.Count != a.ObservableLimits.Count ||
                a.ObservableLimits.Select(l=>l.HasChanges).Any());
            var modifiedLimits = appsToSave.SelectMany(a => a.ObservableLimits)
                .Where(l => l.AppLimitID != default(int) && l.HasChanges);
            var newLimits = appsToSave.SelectMany(a => a.ObservableLimits).Where(l => l.AppLimitID == default(int));
            await dataService.SaveModifiedEntityRangeAsync(modifiedLimits);
            await dataService.SaveNewEntityRangeAsync(newLimits);
            InfoMessage = SETTINGS_SAVED_MSG;
        }

        private void OpenAddNewPopup()
        {
            IsAddNewPopupOpen = !IsAddNewPopupOpen;
        }
    }
}
