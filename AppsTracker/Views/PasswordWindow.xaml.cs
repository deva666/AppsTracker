using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using AppsTracker.Data.Service;
using AppsTracker.Widgets;

namespace AppsTracker.Views
{
    [Export(typeof(IShell))]
    [ExportMetadata("ShellUse", "Password window")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PasswordWindow : Window, IShell
    {
        private readonly ISqlSettingsService settingsService;
        private readonly IWindowService windowService;

        [ImportingConstructor]
        public PasswordWindow(ISqlSettingsService settingsService, IWindowService windowService)
        {
            InitializeComponent();
            this.settingsService = settingsService;
            this.windowService = windowService;
        }
      
        private void lblOK_Click(object sender, RoutedEventArgs e)
        {
            CheckPassword();
        }

        private void lblCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
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
                Close();
                e.Handled = true;
            }
        }

        private void CheckPassword()
        {
            if (Hashing.Hash.GetEncryptedString(pbPassword.Password) == settingsService.Settings.WindowOpen)
            {
                this.DialogResult = true;
                Close();
            }
            else
            {
                windowService.ShowMessageDialog("Wrong password.", false);
            }
        }


        public object ViewArgument
        {
            get;
            set;
        }
    }
}
