using System.ComponentModel.Composition;
using AppsTracker.ViewModels;
using AppsTracker.Widgets;
using System.Windows.Controls;

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
            Loaded += (s, e) =>
            {
                tbMessage.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                tbEmail.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            };
        }


        public object ViewArgument
        {
            get;
            set;
        }
    }
}
