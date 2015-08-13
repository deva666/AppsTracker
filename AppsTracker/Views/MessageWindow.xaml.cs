using System;
using System.ComponentModel.Composition;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;
using AppsTracker.Data.Models;
using AppsTracker.Service.Web;

namespace AppsTracker.Views
{
    [Export(typeof(IShell))]
    [ExportMetadata("ShellUse", "Message window")]
    public partial class MessageWindow : System.Windows.Window, IShell
    {
        private readonly IFeedbackReportService feedbackReportService;

        [ImportingConstructor]
        public MessageWindow(IFeedbackReportService feedbackReportService)
        {
            this.feedbackReportService = feedbackReportService;
            InitializeComponent();
        }

        private void SetMessageText(Exception fail)
        {
            var stringBuilder = new StringBuilder("Ooops, this is awkward ... something went wrong. The app needs to close.");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.Append("Error: ");
            stringBuilder.Append(fail.Message);
            stringBuilder.AppendLine();
            if (fail.InnerException != null)
            {
                stringBuilder.Append("Inner exception: ");
                stringBuilder.Append(fail.InnerException.Message);
            }
            tbMessage.Text = stringBuilder.ToString();
            lblReport.Visibility = System.Windows.Visibility.Visible;
        }


        private void lblOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = true;
            }
            catch (InvalidOperationException)
            {
                //do nothing, window not modal
            }
            Close();
        }

        private void lblCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = false;
            }
            catch (InvalidOperationException)
            {
                //do nothing, window not modal
            }
            Close();
        }

        private async void lblReport_Click(object sender, RoutedEventArgs e)
        {
            var fail = (Exception)ViewArgument;
            var description = fail.Message;
            var stackTrace = fail.StackTrace;
            var email = "info@theappstracker.com";
            var feedback = new Feedback(description, stackTrace, email);
            grdOverlay.Visibility = System.Windows.Visibility.Visible;
            bool success = false;
            try
            {
                success = await feedbackReportService.SendFeedback(feedback);
            }
            catch
            {
                success = false;
            }
            if (success)
            {
                tbInfo.Text = "Crash report sent.";
            }
            else
            {
                tbInfo.Text = "Could not send report, please try again later.";
            }
            grdOverlay.Visibility = System.Windows.Visibility.Collapsed;
        }


        private object viewArgument;

        public object ViewArgument
        {
            get
            {
                return viewArgument;
            }
            set
            {
                if (value is string)
                {
                    tbMessage.Text = (string)value;
                }
                else if (value is Exception)
                {
                    SetMessageText((Exception)value);
                }
                viewArgument = value;
            }
        }
    }
}
