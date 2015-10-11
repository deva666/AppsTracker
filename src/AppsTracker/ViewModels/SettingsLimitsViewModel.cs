using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.Common.Communication;
using AppsTracker.Common.Utils;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class SettingsLimitsViewModel : ViewModelBase
    {
        private const string SETTINGS_SAVED_MSG = "settings saved";

        private readonly IDataService dataService;
        private readonly ITrackingService trackingService;
        private readonly IMediator mediator;
        private readonly IWorkQueue workQueue;

        private readonly ICollection<AppLimit> limitsToDelete = new List<AppLimit>();


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


        private AppLimit selectedLimit;

        public AppLimit SelectedLimit
        {
            get { return selectedLimit; }
            set { SetPropertyValue(ref selectedLimit, value); }
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
        public SettingsLimitsViewModel(IDataService dataService,
                                       ITrackingService trackingService,
                                       IMediator mediator,
                                       IWorkQueue workQueue)
        {
            this.dataService = dataService;
            this.trackingService = trackingService;
            this.mediator = mediator;
            this.workQueue = workQueue;

            mediator.Register(MediatorMessages.APPLICATION_ADDED, new Action<Aplication>(OnAppAdded));

            AppList = new AsyncProperty<IEnumerable<Aplication>>(GetApps, this);
        }

        private void OnAppAdded(Aplication app)
        {
            AppList.Reload();
        }

        private IEnumerable<Aplication> GetApps()
        {
            var apps = dataService.GetFiltered<Aplication>(a => a.User.UserID == trackingService.SelectedUserID,
                                                           a => a.Limits)
                                                        .ToList()
                                                        .Distinct();
            foreach (var app in apps)
            {
                app.ObservableLimits = new ObservableCollection<AppLimit>(app.Limits);
            }

            return apps;
        }


        private void AddNewLimit(object parameter)
        {
            if (selectedApp == null)
                return;

            string limitSpan = (string)parameter;
            var appLimit = new AppLimit() { ApplicationID = selectedApp.ApplicationID };

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

            var appsToSave = apps.Where(a => a.Limits.Count != a.ObservableLimits.Count ||
                a.ObservableLimits.Select(l => l.HasChanges).Any());
            var modifiedLimits = appsToSave.SelectMany(a => a.ObservableLimits)
                .Where(l => l.AppLimitID != default(int) && l.HasChanges);
            var newLimits = appsToSave.SelectMany(a => a.ObservableLimits).Where(l => l.AppLimitID == default(int));
            var saveModifiedTask = workQueue.EnqueueWork(() => dataService.SaveModifiedEntityRange(modifiedLimits));
            var saveNewTask = workQueue.EnqueueWork(() => dataService.SaveNewEntityRange(newLimits));
            var deleteTask = workQueue.EnqueueWork(() => dataService.DeleteEntityRange(limitsToDelete.Where(l => l.AppLimitID != default(int))));

            await Task.WhenAll(saveModifiedTask, saveNewTask, deleteTask);

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
