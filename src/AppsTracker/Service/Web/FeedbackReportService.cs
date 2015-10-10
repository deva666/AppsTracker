using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using AppsTracker.Common.Utils;
using AppsTracker.Data.Models;

namespace AppsTracker.Service.Web
{
    [Export(typeof(IFeedbackReportService))]
    public sealed class FeedbackReportService : IFeedbackReportService
    {
        private const string SERVER_URI = "http://www.theappstracker.com/bug/";

        public async Task<bool> SendFeedback(Feedback feedback)
        {
            Ensure.NotNull(feedback, "feedback");

            ServicePointManager.Expect100Continue = false;

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(SERVER_URI);
            httpWebRequest.Proxy = WebRequest.DefaultWebProxy;
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.UseDefaultCredentials = true;

            using (var streamWriter = new StreamWriter(await httpWebRequest.GetRequestStreamAsync()))
            {
                var json = new JavaScriptSerializer().Serialize(new
                {
                    description = feedback.Description,
                    stack_trace = feedback.StackTrace,
                    reported_by = feedback.ReporterEmail
                });

                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync();
            httpResponse.Close();

            return httpResponse.StatusCode == HttpStatusCode.Created;
        }
    }
}
