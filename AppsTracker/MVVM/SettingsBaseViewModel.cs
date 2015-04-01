#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.Service;

namespace AppsTracker.ServiceLocation
{
    internal abstract class SettingsBaseViewModel : ViewModelBase
    {
        private const string SETTINGS_SAVED_MSG = "settings saved";

        private ISqlSettingsService settingsService;

        
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
            settingsService = serviceResolver.Resolve<ISqlSettingsService>();
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
