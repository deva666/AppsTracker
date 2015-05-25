using AppsTracker.Data.Models;

namespace AppsTracker.Data.Service
{
    public sealed class FeedbackReportService
    {
        private const string SERVER_URI = "http://www.theappstracker.com/bug";

        public bool SendFeedback(Feedback feedback)
        {
            return false;
        }
    }
}
