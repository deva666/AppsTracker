using System.Threading.Tasks;
using AppsTracker.Data.Models;

namespace AppsTracker.Service.Web
{
    public interface IFeedbackReportService
    {
        Task<bool> SendFeedback(Feedback feedback);
    }
}
