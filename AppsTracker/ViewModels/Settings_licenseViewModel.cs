//#region Licence
///*
//  *  Author: Marko Devcic, madevcic@gmail.com
//  *  Copyright: Marko Devcic, 2014
//  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
// */
//#endregion

//using System;
//using System.Diagnostics;
//using System.Reflection;
//using System.Windows;
//using System.Windows.Views;
//using System.Windows.Input;

//using AppsTracker.Views;
//using AppsTracker.MVVM;


//namespace AppsTracker.ViewModels
//{
//    internal sealed class Settings_licenseViewModel : ViewModelBase
//    {
//        #region Fields

//        private bool _isPopupLicenseOpen = false;

//        private ICommand _applyLicenseCommand;
//        private ICommand _showHidePopupLicenceCommand;
//        private ICommand _getLicenseCommand;

//        #endregion

//        #region Properties

//        public override string Title { get { return "LICENCE"; } }
//        public bool IsContentLoaded { get; private set; }

//        public bool IsPopupLicenseOpen
//        {
//            get { return _isPopupLicenseOpen; }
//            set { _isPopupLicenseOpen = value; PropertyChanging("IsPopupLicenseOpen"); }
//        }

//        public bool Licence
//        {
//            get { return App.UzerSetting.Licence; }
//            set { App.UzerSetting.Licence = value; PropertyChanging("Licence"); }
//        }

//        public SettingsProxy UserSettings { get { return App.UzerSetting; } }

//        public ICommand ApplyLicenseCommand
//        {
//            get { if (_applyLicenseCommand == null) _applyLicenseCommand = new DelegateCommand(ApplyLicense); return _applyLicenseCommand; }
//        }

//        public ICommand ShowHidePopupLicenceCommand
//        {
//            get { if (_showHidePopupLicenceCommand == null) _showHidePopupLicenceCommand = new DelegateCommand(ShowHidePopup); return _showHidePopupLicenceCommand; }
//        }

//        public ICommand GetLicenseCommand
//        {
//            get { if (_getLicenseCommand == null) _getLicenseCommand = new DelegateCommand(GetLicense); return _getLicenseCommand; }
//        }

//        #endregion

//        #region Command Methods

//        private void ApplyLicense(object parameter)
//        {
//            var textboxes = parameter as TextBox[];
//            if (textboxes != null)
//            {
//                string username = textboxes[0].Text;
//                string license = textboxes[1].Text;
//                string verify = string.Format("{0} {1} {2}", username.Trim(), Constants.APP_NAME, Assembly.GetExecutingAssembly().GetName().Version.Major.ToString());
//                if (license == Hashing.Hash.GetEncryptedString(verify))
//                {
//                    CreateLicence(username, license);
//                }
//                else
//                {
//                    MessageWindow messageWindow = new MessageWindow("Invalid license key.");
//                    messageWindow.Owner = Application.Current.MainWindow;
//                    messageWindow.ShowDialog();
//                }

//            }
//        }

//        private void ShowHidePopup()
//        {
//            if (IsPopupLicenseOpen)
//                IsPopupLicenseOpen = false;
//            else
//                IsPopupLicenseOpen = true;
//        }

//        private void GetLicense()
//        {
//            Process.Start("http://www.theappstracker.com");
//        }

//        #endregion

//        #region Licensing Methods

//        private void CreateLicence(string username, string license)
//        {
//            string serialized = username + Environment.NewLine + license;
//            UserSettings.Username = username;
//            Licence = true;
//            ShowHidePopup();
//        }

//        #endregion


//    }
//}
