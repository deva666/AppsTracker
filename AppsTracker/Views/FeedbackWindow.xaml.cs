using System.ComponentModel.Composition;
using AppsTracker.ViewModels;
using AppsTracker.Widgets;

namespace AppsTracker.Views
{
    [Export(typeof(IShell))]
    [ExportMetadata("ShellUse", "Feedback window")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class FeedbackWindow : System.Windows.Window, IShell
    {
        [ImportingConstructor]
        public FeedbackWindow(FeedbackReportViewModel viewModel)
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
