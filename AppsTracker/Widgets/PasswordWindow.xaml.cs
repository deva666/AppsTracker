using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using AppsTracker.Service;
using AppsTracker.ServiceLocation;
using AppsTracker.Widgets;

namespace AppsTracker
{
    [Export(typeof(IPasswordWindow))]
    public partial class PasswordWindow : Window, IPasswordWindow
    {
        private readonly ISqlSettingsService settingService;
        private readonly IWindowService windowService;

        public PasswordWindow()
        {
            InitializeComponent();
            settingService = ServiceLocator.Instance.Resolve<ISqlSettingsService>();
            windowService = ServiceLocator.Instance.Resolve<IWindowService>();
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

        private void CheckPassword()
        {
            if (Hashing.Hash.GetEncryptedString(pbPassword.Password) == settingService.Settings.WindowOpen)
            {
                this.DialogResult = true;
                FadeUnloaded();
            }
            else
            {
                windowService.ShowDialog("Wrong password.", false);
            }
        }
    }
}
