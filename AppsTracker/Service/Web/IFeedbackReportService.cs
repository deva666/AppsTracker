using System;
using System.Threading.Tasks;
using AppsTracker.Data.Models;

namespace AppsTracker.Service.Web
{
    interface IFeedbackReportService
    {
        Task<bool> SendFeedback(Feedback feedback);
    }
}
