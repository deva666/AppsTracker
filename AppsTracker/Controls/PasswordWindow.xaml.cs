using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using AppsTracker.Controls;
using AppsTracker.DAL.Service;
using AppsTracker.Logging;
using AppsTracker.Models.Proxy;


namespace AppsTracker
{
    public partial class PasswordWindow : Window
    {
        ISettingsService _settingService;

        public PasswordWindow()
        {
            InitializeComponent();
            _settingService = ServiceFactory.Get<ISettingsService>();
            SetKeylogger(false);
            this.Closing += (s, e) => SetKeylogger(true);
        }

        private void FadeUnloaded()
        {
            DoubleAnimation fadeOut = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromSeconds(0.6)));

            fadeOut.SetValue(Storyboard.TargetProperty, this);

            Storyboard story = new Storyboard();
            Storyboard.SetTarget(fadeOut, this);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));

            story.Children.Add(fadeOut);
            story.Completed += (s, e) => { this.Close(); };
            story.Begin(this);
        }

        #region Event Handlers

        private void lblOK_Click(object sender, RoutedEventArgs e)
        {
            CheckPassword();
        }

        private void lblCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            FadeUnloaded();
        }

        private void Window_PreviewKeyUp_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CheckPassword();
                e.Handled = true;
            }

            if (e.Key == Key.Escape)
            {
                FadeUnloaded();
                e.Handled = true;
            }
        }
        #endregion

        #region Class Methods

        private void SetKeylogger(bool enabled)
        {
            //if (App.UzerSetting.LoggingEnabled && App.UzerSetting.EnableKeylogger)
            //    App.Container.SetKeyboardHookEnabled(enabled);
        }

        private void CheckPassword()
        {
            if (Encryption.Encrypt.GetEncryptedString(pbPassword.Password) == _settingService.Settings.WindowOpen)
            {
                this.DialogResult = true;
                FadeUnloaded();
            }
            else
            {
                MessageWindow messageWindow = new MessageWindow("Wrong password.");
                messageWindow.Owner = this;
                messageWindow.ShowDialog();
            }
        }
        #endregion


    }
}
