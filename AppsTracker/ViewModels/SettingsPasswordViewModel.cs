using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using AppsTracker.Controls;
using AppsTracker.DAL;
using AppsTracker.Encryption;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal sealed class SettingsPasswordViewModel : SettingsBaseViewModel
    {
        public override string Title
        {
            get { return "MASTER PASSWORD"; }
        }

        private ICommand _setPasswordCommand;
        public ICommand SetPasswordCommand
        {
            get
            {
                return _setPasswordCommand ?? (_setPasswordCommand = new DelegateCommand(SetPassword));
            }
        }

        private void SetPassword(object parameter)
        {
            var passwords = parameter as PasswordBox[];

            if (passwords == null)
                return;

            if (Settings.IsMasterPasswordSet)
            {
                string currentPassword = Encrypt.GetEncryptedString(passwords[2].Password);
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
                    MessageWindow messageWindow = new MessageWindow("Wrong current password.");
                    messageWindow.ShowDialog();
                    return;
                }
            }
            string password = passwords[0].Password;
            string confirmPassword = passwords[1].Password;
            if (password != confirmPassword)
            {
                MessageWindow messageWindow = new MessageWindow("Passwords don't match.");
                messageWindow.ShowDialog();
                return;
            }
            if (!string.IsNullOrEmpty(password.Trim()))
            {
                Settings.IsMasterPasswordSet = true;
                Settings.WindowOpen = Encrypt.GetEncryptedString(password);
                MessageWindow messageWindow = new MessageWindow("Password set.");
                messageWindow.ShowDialog();
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
