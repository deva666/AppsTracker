using System.IO;
using System.Net;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Common.Utils;

namespace AppsTracker.Data.Service
{
    public sealed class FeedbackReportService
    {
        private const string SERVER_URI = "http://www.theappstracker.com/bug";

        public async Task<bool> SendFeedback(Feedback feedback)
        {
            Ensure.NotNull(feedback, "feedback");

            var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(SERVER_URI);
            httpWebRequest.Proxy = WebRequest.DefaultWebProxy;
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(await httpWebRequest.GetRequestStreamAsync()))
            {
                //var json = "{\"username\":\"" + settings.UserName + "\"," +
                //            "\"password\":\"" + settings.Password + "\"," +
                //            "\"clientid\":\"" + settings.ClientId + "\"}";
                var json = "";

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
               
            }
            return false;
        }
    }
}
