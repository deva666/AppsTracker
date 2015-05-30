using System.ComponentModel.Composition;
using System.Windows;
using AppsTracker.ViewModels;
using AppsTracker.Widgets;

namespace AppsTracker.Views
{
    [Export(typeof(IShell))]
    [ExportMetadata("ShellUse", "Release notes window")]
    public partial class ReleaseNotesWindow : Window, IShell
    {
        [ImportingConstructor]
        public ReleaseNotesWindow(ExportFactory<ReleaseNotesViewModel> viewModelFactory)
        {
            InitializeComponent();

            using (var context = viewModelFactory.CreateExport())
            {
                DataContext = context.Value;
            }
        }


        public object ViewArgument
        {
            get;
            set;
        }

        private void CloseButton_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
