#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using AppsTracker.Data.Repository;
using AppsTracker.Service;


namespace AppsTracker.Views
{
    [Export(typeof(IShell))]
    [ExportMetadata("ShellUse", "DbCleaner window")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class DBCleanerWindow : Window, IShell
    {
        private readonly IRepository repository;
        private readonly IWindowService windowService;

        [ImportingConstructor]
        public DBCleanerWindow(IRepository repository,
                               IWindowService windowService)
        {
            InitializeComponent();
            this.repository = repository;
            this.windowService = windowService;
        }

        private void lblCancel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private async void lblClean_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int days;
            if (!int.TryParse(tbDays.Text, out days))
                return;

            overlayGrd.Visibility = Visibility.Visible;
            int count = await repository.DeleteOldScreenshotsAsync(days);
            overlayGrd.Visibility = Visibility.Collapsed;
            windowService.ShowMessageDialog(string.Format("Deleted {0} screenshots", count));
            Close();
        }

        private void tbDays_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        public object ViewArgument
        {
            get;
            set;
        }
    }
}
