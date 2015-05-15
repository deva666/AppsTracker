using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using AppsTracker.Service;
using AppsTracker.ServiceLocation;
using AppsTracker.Widgets;

namespace AppsTracker
{
    [Export(typeof(IShell))]
    [ExportMetadata("ShellUse", "Password window")]
    public partial class PasswordWindow : Window, IShell
    {
        private readonly ISqlSettingsService settingService;
        private readonly IWindowService windowService;

        [ImportingConstructor]
        public PasswordWindow(ISqlSettingsService settingsService, IWindowService windowService)
        {
            InitializeComponent();
            this.settingService = settingService;
            this.windowService = windowService;
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
                windowService.ShowMessageDialog("Wrong password.", false);
            }
        }
    }
}
