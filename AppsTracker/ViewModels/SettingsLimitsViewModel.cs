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
        public override string Title
        {
            get { return "APP LIMITS"; }
        }

        private readonly IDataService dataService;
        private readonly ILoggingService loggingService;


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
                    (saveChangesCommand = new DelegateCommand(SaveChanges));
            }
        }


        [ImportingConstructor]
        public SettingsLimitsViewModel(IDataService dataService,
                                       ILoggingService loggingService)
        {
            this.dataService = dataService;
            this.loggingService = loggingService;

            AppList = new AsyncProperty<IEnumerable<Aplication>>(GetApps, this);
        }

        private IEnumerable<Aplication> GetApps()
        {
            var apps = dataService.GetFiltered<Aplication>(a => a.User.UserID == loggingService.SelectedUserID
                                                             && a.Windows.SelectMany(w => w.Logs).Where(l => l.DateCreated >= loggingService.DateFrom).Any()
                                                             && a.Windows.SelectMany(w => w.Logs).Where(l => l.DateCreated <= loggingService.DateTo).Any(),
                                                             a => a.Limits)
                                                        .ToList()
                                                        .Distinct();
            foreach (var app in apps)
            {
                app.ObservableLimits = new System.Collections.ObjectModel.ObservableCollection<AppLimit>(app.Limits);
            }

            return apps;
        }


        private void AddNewLimit()
        {
            if (selectedApp == null)
                return;

            selectedApp.ObservableLimits.Add(new AppLimit());

        }

        private void SaveChanges()
        {
            
        }

    }
}
