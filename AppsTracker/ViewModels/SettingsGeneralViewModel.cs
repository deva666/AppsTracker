#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Windows.Input;
using AppsTracker.MVVM;
using AppsTracker.Service;
using AppsTracker.Widgets;
using Microsoft.Win32;

namespace AppsTracker.ViewModels
{
    internal sealed class SettingsGeneralViewModel : SettingsBaseViewModel
    {
        public override string Title
        {
            get { return "GENERAL"; }
        }


        private ICommand showAboutWindowCommand;

        public ICommand ShowAboutWindowCommand
        {
            get { return showAboutWindowCommand ?? (showAboutWindowCommand = new DelegateCommand(ShowAboutWindow)); }
        }


        private ICommand setStartupCommand;

        public ICommand SetStartupCommand
        {
            get { return setStartupCommand ?? (setStartupCommand = new DelegateCommand(SetStartup)); }
        }


        private ICommand changeThemeCommand;

        public ICommand ChangeThemeCommand
        {
            get { return changeThemeCommand == null ? changeThemeCommand = new DelegateCommand(ChangeTheme) : changeThemeCommand; }
        }


        public SettingsGeneralViewModel() : base() { }


        private void ShowAboutWindow()
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }


        private void SetStartup()
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (!Settings.RunAtStartup)
                {
                    rk.SetValue("app service", System.Reflection.Assembly.GetExecutingAssembly().Location + " -autostart");
                    Settings.RunAtStartup = true;
                }
                else
                {
                    rk.DeleteValue("app service", false);
                    Settings.RunAtStartup = false;
                }
            }
            catch (System.Security.SecurityException)
            {
                serviceResolver.Resolve<IMessageService>().ShowDialog("You don't have administrative privilages to change this option."
                                        + Environment.NewLine + "Please try running the app as Administrator." + Environment.NewLine
                                        + "Right click on the app or shortcut and select 'Run as Adminstrator'.", false);
            }
            SettingsChanging();
        }


        private void ChangeTheme(object parameter)
        {
            if ((parameter as string) == "Light")
                Settings.LightTheme = true;
            else if ((parameter as string) == "Dark")
                Settings.LightTheme = false;
        }
    }
}
