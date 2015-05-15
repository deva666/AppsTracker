#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Input;
using AppsTracker.Hashing;
using AppsTracker.MVVM;
using AppsTracker.Service;

namespace AppsTracker.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.Any)]
    public sealed class SettingsPasswordViewModel : SettingsBaseViewModel
    {
        private readonly IWindowService windowService;

        public override string Title
        {
            get { return "MASTER PASSWORD"; }
        }


        [ImportingConstructor]
        public SettingsPasswordViewModel(IWindowService windowService, ISqlSettingsService settingsService) 
            : base(settingsService)
        {
            this.windowService = windowService;
        }

        private ICommand setPasswordCommand;

        public ICommand SetPasswordCommand
        {
            get { return setPasswordCommand ?? (setPasswordCommand = new DelegateCommand(SetPassword)); }
        }


        private void SetPassword(object parameter)
        {
            var passwords = parameter as PasswordBox[];

            if (passwords == null)
                return;

            if (Settings.IsMasterPasswordSet)
            {
                string currentPassword = Hash.GetEncryptedString(passwords[2].Password);
                string storedPassword = Settings.WindowOpen;
                if (storedPassword == null)
                {
                    Settings.IsMasterPasswordSet = false;
                    SettingsChanging();
                    SaveChangesCommand.Execute(null);
                    return;
                }
                if (currentPassword != storedPassword)
                {
                    windowService.ShowMessageDialog("Wrong current password.", false);
                    return;
                }
            }
            string password = passwords[0].Password;
            string confirmPassword = passwords[1].Password;
            if (password != confirmPassword)
            {
                windowService.ShowMessageDialog("Passwords don't match", false);
                return;
            }
            if (!string.IsNullOrEmpty(password.Trim()))
            {
                Settings.IsMasterPasswordSet = true;
                Settings.WindowOpen = Hash.GetEncryptedString(password);
                windowService.ShowMessageDialog("Password set.", false);
            }
            else
            {
                Settings.IsMasterPasswordSet = false;
                Settings.WindowOpen = "";
            }
            SettingsChanging();
            SaveChangesCommand.Execute(null);
        }
    }
}
