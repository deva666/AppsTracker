using System.ComponentModel.Composition;
using System.Windows;
using AppsTracker.ViewModels;

namespace AppsTracker.Views
{
    [Export(typeof(IShell))]
    [ExportMetadata("ShellUse", "Release notes window")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ReleaseNotesWindow : Window, IShell
    {
        [ImportingConstructor]
        public ReleaseNotesWindow(ReleaseNotesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }


        public object ViewArgument
        {
            get;
            set;
        }
    }
}
