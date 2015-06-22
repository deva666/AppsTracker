using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using AppsTracker.Common.Utils;
using AppsTracker.Data.Models;

namespace AppsTracker.Service.Web
{
    [Export(typeof(IFeedbackReportService))]
    public sealed class FeedbackReportService : IFeedbackReportService
    {
#if DEBUG
        private const string SERVER_URI = "http://localhost:8000/bug";
#else
        private const string SERVER_URI = "http://www.theappstracker.com/bug";
#endif

        public async Task<bool> SendFeedback(Feedback feedback)
        {
            Ensure.NotNull(feedback, "feedback");

            var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(SERVER_URI);
            httpWebRequest.Proxy = WebRequest.DefaultWebProxy;
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(await httpWebRequest.GetRequestStreamAsync()))
            {
                var json = "{\"description\":\"" + feedback.Description + "\"," +
                            "\"stack_trace\":\"" + feedback.StackTrace + "\"," +
                            "\"reported_by\":\"" + feedback.ReporterEmail + "\"}";

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync();
            httpResponse.Close();

            if (httpResponse.StatusCode == HttpStatusCode.Created)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
