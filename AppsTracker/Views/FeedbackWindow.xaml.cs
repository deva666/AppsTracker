using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AppsTracker.Service.Web;
using AppsTracker.ViewModels;
using AppsTracker.Widgets;

namespace AppsTracker.Views
{
    [Export(typeof(IShell))]
    [ExportMetadata("ShellUse", "Feedback window")]
    public partial class FeedbackWindow : Window, IShell
    {
        private readonly IFeedbackReportService feedbackReportService;

        [ImportingConstructor]
        public FeedbackWindow(FeedbackReportViewModel viewModel,
                              IFeedbackReportService feedbackReportService)
        {
            InitializeComponent();

            this.feedbackReportService = feedbackReportService;
            DataContext = viewModel;
        }


        public object ViewArgument
        {
            get;
            set;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
