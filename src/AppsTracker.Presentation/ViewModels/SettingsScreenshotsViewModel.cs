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
using AppsTracker.Common.Communication;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Settings;
using AppsTracker.Domain.Tracking;
using AppsTracker.MVVM;
using AppsTracker.Service;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class SettingsScreenshotsViewModel : SettingsBaseViewModel
    {
        private readonly ITrackingService trackingService;
        private readonly IWindowService windowService;
        private readonly IRepository repository;

        public override string Title
        {
            get { return "SCREENSHOTS"; }
        }


        private bool popupIntervalIsOpen = false;

        public bool PopupIntervalIsOpen
        {
            get { return popupIntervalIsOpen; }
            set { SetPropertyValue(ref popupIntervalIsOpen, value); }
        }


        private ICommand changeScreenshotsCommand;

        public ICommand ChangeScreenshotsCommand
        {
            get
            {
                return changeScreenshotsCommand ?? (changeScreenshotsCommand = new DelegateCommand(ChangeScreenshots, o => repository.DBSizeOperational));
            }
        }


        private ICommand changeScreenshotIntervalCommand;

        public ICommand ChangeScreenShotIntervalCommand
        {
            get
            {
                return changeScreenshotIntervalCommand ?? (changeScreenshotIntervalCommand = new DelegateCommand(ChangeScreenshotInterval));
            }
        }


        private ICommand showPopUpCommand;

        public ICommand ShowPopupCommand
        {
            get
            {
                return showPopUpCommand ?? (showPopUpCommand = new DelegateCommand(ShowPopUp));
            }
        }


        private ICommand showFolderBrowserDialogCommand;

        public ICommand ShowFolderBrowserDialogCommand
        {
            get
            {
                return showFolderBrowserDialogCommand ?? (showFolderBrowserDialogCommand = new DelegateCommand(ShowFolderBrowserDialog));
            }
        }


        private ICommand runDBCleanerCommand;

        public ICommand RunDBCleanerCommand
        {
            get
            {
                return runDBCleanerCommand ?? (runDBCleanerCommand = new DelegateCommand(RunDBCleaner));
            }
        }


        [ImportingConstructor]
        public SettingsScreenshotsViewModel(IAppSettingsService settingsService,
                                            ITrackingService trackingService,
                                            IRepository repository,
                                            IWindowService windowService,
                                            Mediator mediator)
            : base(settingsService, mediator)
        {
            this.trackingService = trackingService;
            this.repository = repository;
            this.windowService = windowService;
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
            PopupIntervalIsOpen = !popupIntervalIsOpen;
        }


        private void ShowFolderBrowserDialog(object parameter)
        {
            string path;

            var dialog = windowService.CreateFolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                path = dialog.SelectedPath;
            else
                return;

            Settings.DefaultScreenshotSavePath = path;
            SettingsChanging();
        }


        private void RunDBCleaner()
        {
            windowService.ShowShell("DbCleaner window");
        }
    }
}
