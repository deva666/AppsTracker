using System.ComponentModel.Composition;
using System.Windows;
using AppsTracker.ViewModels;

namespace AppsTracker.Views
{
    [Export(typeof(IShell))]
    [ExportMetadata("ShellUse", "About window")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class AboutWindow : Window, IShell
    {
        [ImportingConstructor]
        public AboutWindow(AboutWindowViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        public object ViewArgument
        {
            get;
            set;
        }
    }
}
