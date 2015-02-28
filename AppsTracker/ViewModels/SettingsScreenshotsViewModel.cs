using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using AppsTracker.Controls;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal sealed class SettingsScreenshotsViewModel : SettingsBaseViewModel
    {
        public override string Title
        {
            get { return "SCREENSHOTS"; }
        }

        private bool _popupIntervalIsOpen = false;
        public bool PopupIntervalIsOpen
        {
            get
            {
                return _popupIntervalIsOpen;
            }
            set
            {
                _popupIntervalIsOpen = value;
                PropertyChanging("PopupIntervalIsOpen");
            }
        }

        private ICommand _changeScreenshotsCommand;
        public ICommand ChangeScreenshotsCommand
        {
            get
            {
                return _changeScreenshotsCommand ?? (_changeScreenshotsCommand = new DelegateCommand(ChangeScreenshots, o => Globals.DBSizeOperational));
            }
        }

        private ICommand _changeScreenshotIntervalCommand;
        public ICommand ChangeScreenShotIntervalCommand
        {
            get
            {
                return _changeScreenshotIntervalCommand ?? (_changeScreenshotIntervalCommand = new DelegateCommand(ChangeScreenshotInterval));
            }
        }

        private ICommand _showPopUpCommand;
        public ICommand ShowPopupCommand
        {
            get
            {
                return _showPopUpCommand ?? (_showPopUpCommand = new DelegateCommand(ShowPopUp));
            }
        }

        private ICommand _showFolderBrowserDialogCommand;
        public ICommand ShowFolderBrowserDialogCommand
        {
            get
            {
                return _showFolderBrowserDialogCommand ?? (_showFolderBrowserDialogCommand = new DelegateCommand(ShowFolderBrowserDialog));
            }
        }

        private ICommand _runDBCleanerCommand;
        public ICommand RunDBCleanerCommand
        {
            get
            {
                return _runDBCleanerCommand ??( _runDBCleanerCommand = new DelegateCommand(RunDBCleaner));
            }
        }
        private void ChangeScreenshots()
        {
            Settings.TakeScreenshots = !Settings.TakeScreenshots;
        }

        private void ChangeScreenshotInterval(object sourceLabel)
        {
            System.Windows.Controls.Label label = sourceLabel as Label;
            if (label != null)
            {
                switch (label.Content as string)
                {
                    case "10 sec":
                        Settings.ScreenshotInterval = ScreenShotInterval.TenSeconds;
                        break;
                    case "30 sec":
                        Settings.ScreenshotInterval = ScreenShotInterval.ThirtySeconds;
                        break;
                    case "1 min":
                        Settings.ScreenshotInterval = ScreenShotInterval.OneMinute;
                        break;
                    case "2 min":
                        Settings.ScreenshotInterval = ScreenShotInterval.TwoMinute;
                        break;
                    case "5 min":
                        Settings.ScreenshotInterval = ScreenShotInterval.FiveMinute;
                        break;
                    case "10 min":
                        Settings.ScreenshotInterval = ScreenShotInterval.TenMinute;
                        break;
                    case "30 min":
                        Settings.ScreenshotInterval = ScreenShotInterval.ThirtyMinute;
                        break;
                    case "1 hr":
                        Settings.ScreenshotInterval = ScreenShotInterval.OneHour;
                        break;
                    default:
                        break;
                }
                PopupIntervalIsOpen = false;
                SettingsChanging();
            }
        }

        private void ShowPopUp(object popupSource)
        {
            PopupIntervalIsOpen = !_popupIntervalIsOpen;
        }

        private void ShowFolderBrowserDialog(object parameter)
        {
            string path;

            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                path = dialog.SelectedPath;
            else
                return;

            Settings.DefaultScreenshotSavePath = path;
            SettingsChanging();
        }

        private void RunDBCleaner()
        {
            DBCleanerWindow dbCleanerWindow = new DBCleanerWindow();
            dbCleanerWindow.ShowDialog();
        }
    }
}
