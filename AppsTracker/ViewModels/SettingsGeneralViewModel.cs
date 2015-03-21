using AppsTracker.Views;
using AppsTracker.MVVM;
using Microsoft.Win32;
using System;
using System.Windows.Input;

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
                MessageWindow messageWindow = new MessageWindow("You don't have administrative privilages to change this option."
                                        + Environment.NewLine + "Please try running the app as Administrator." + Environment.NewLine
                                        + "Right click on the app or shortcut and select 'Run as Adminstrator'.");
                messageWindow.ShowDialog();
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
