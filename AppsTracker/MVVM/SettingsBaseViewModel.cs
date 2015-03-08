using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.Controllers;
using AppsTracker.Data.Service;
using AppsTracker.Data.Models;

namespace AppsTracker.MVVM
{
    internal abstract class SettingsBaseViewModel : ViewModelBase
    {
        private const string SETTINGS_SAVED_MSG = "settings saved";

        private ISqlSettingsService settingsService;

        private string infoMessage;
        public string InfoMessage
        {
            get { return infoMessage; }
            set { SetPropertyValue(ref infoMessage, value); }
        }        

        private Setting settings;
        public Setting Settings
        {
            get { return settings; }
            set { SetPropertyValue(ref settings, value); }
        }

        private ICommand saveChangesCommand;
        public ICommand SaveChangesCommand
        {
            get { return saveChangesCommand ?? (saveChangesCommand = new DelegateCommandAsync(SaveChangesAsync)); }
        }
        public SettingsBaseViewModel()
        {
            settingsService = ServiceFactory.Get<ISqlSettingsService>();
            Settings = settingsService.Settings;
        }

        private void SaveChanges()
        {
            settingsService.SaveChanges(settings);
            InfoMessage = SETTINGS_SAVED_MSG;
        }

        private async Task SaveChangesAsync()
        {
            await settingsService.SaveChangesAsync(settings);
            InfoMessage = SETTINGS_SAVED_MSG;
        }

        protected void SettingsChanging()
        {
            PropertyChanging("Settings");
        }
    }
}
