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
using System.ComponentModel.Composition;

namespace AppsTracker.MVVM
{
    public abstract class SettingsBaseViewModel : ViewModelBase
    {
        private const string SETTINGS_SAVED_MSG = "settings saved";

        private readonly ISqlSettingsService settingsService;
        private readonly IMediator mediator;
                
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


        public SettingsBaseViewModel(ISqlSettingsService settingsService, IMediator mediator)
        {
            this.settingsService = settingsService;
            this.mediator = mediator;

            this.mediator.Register(MediatorMessages.RELOAD_SETTINGS, ReloadSettings);
            Settings = this.settingsService.Settings;
        }

        private void ReloadSettings()
        {
            Settings = settingsService.Settings; ;
        }


        private void SaveChanges()
        {
            settingsService.SaveChanges(settings);
            mediator.NotifyColleagues(MediatorMessages.RELOAD_SETTINGS);
            InfoMessage = SETTINGS_SAVED_MSG;
        }


        private async Task SaveChangesAsync()
        {
            await settingsService.SaveChangesAsync(settings);
            mediator.NotifyColleagues(MediatorMessages.RELOAD_SETTINGS);
            InfoMessage = SETTINGS_SAVED_MSG;
        }


        protected void SettingsChanging()
        {
            PropertyChanging("Settings");
        }
    }
}
