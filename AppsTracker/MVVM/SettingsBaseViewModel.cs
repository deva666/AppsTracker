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
        private ISqlSettingsService _settingsService;

        private Setting _settings;
        public Setting Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;
                PropertyChanging("Settings");
            }
        }

        private ICommand _saveChangesCommand;
        public ICommand SaveChangesCommand
        {
            get
            {
                return _saveChangesCommand ?? (_saveChangesCommand = new DelegateCommandAsync(SaveChangesAsync));
            }
        }
        public SettingsBaseViewModel()
        {
            _settingsService = ServiceFactory.Get<ISqlSettingsService>();
            Settings = _settingsService.Settings;
        }

        private void SaveChanges()
        {
            _settingsService.SaveChanges(_settings);
        }

        private async Task SaveChangesAsync()
        {
           await _settingsService.SaveChangesAsync(_settings);
        }

        protected void SettingsChanging()
        {
            PropertyChanging("Settings");
        }
    }
}
