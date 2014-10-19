using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Task_Logger_Pro.Controls;


namespace Task_Logger_Pro
{
    public partial class PasswordWindow : Window
    {

        public PasswordWindow()
        {
            InitializeComponent();
            if (App.DataLogger.KeyBoardHook != null)
                App.DataLogger.KeyBoardHook.KeyLoggerEnabled = false;
            this.Closing += (s, e) =>
            {
                if (App.UzerSetting.EnableKeylogger && App.DataLogger.KeyBoardHook != null) 
                    App.DataLogger.KeyBoardHook.KeyLoggerEnabled = true;
            };
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

        private void CheckPassword()
        {
            if (Encryption.Encrypt.GetEncryptedString(pbPassword.Password) == App.UzerSetting.WindowOpen)
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
