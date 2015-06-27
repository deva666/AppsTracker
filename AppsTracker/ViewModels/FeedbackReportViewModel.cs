using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Service.Web;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Any)]
    public sealed class FeedbackReportViewModel : ViewModelBase
    {
        private readonly IFeedbackReportService feedbackReportService;


        public override string Title
        {
            get { return "feedback report"; }
        }


        private string feedbackText;

        public string FeedbackText
        {
            get { return feedbackText; }
            set { SetPropertyValue(ref feedbackText, value); }
        }


        private string email;

        public string Email
        {
            get { return email; }
            set { SetPropertyValue(ref email, value); }
        }


        private string infoMessage;

        public string InfoMessage
        {
            get { return infoMessage; }
            set
            {
                SetPropertyValue(ref infoMessage, string.Empty);
                SetPropertyValue(ref infoMessage, value);
            }
        }


        private ICommand sendFeedbackCommand;

        public ICommand SendFeedbackCommand
        {
            get { return sendFeedbackCommand ?? (sendFeedbackCommand = new DelegateCommandAsync(SendFeedback)); }
        }


        [ImportingConstructor]
        public FeedbackReportViewModel(IFeedbackReportService _feedbackReportService)
        {
            feedbackReportService = _feedbackReportService;
        }


        private async Task SendFeedback()
        {
            var feedback = new Feedback(feedbackText, "", email);
            Working = true;
            bool success = false;
            try
            {
                success = await feedbackReportService.SendFeedback(feedback);
            }
            catch
            {
                success = false;
            }
            InfoMessage = success ? "Feedback sent" : "Failed to send report. Please try again.";
            Working = false;
        }
    }
}
