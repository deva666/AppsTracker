using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using AppsTracker.Data.Models;

namespace AppsTracker.Service.Web
{
    [Export(typeof(IReleaseNotesService))]
    public sealed class ReleaseNotesService : IReleaseNotesService
    {
        private const string SERVER_URI = "http://www.theappstracker.com/release_notes";

        private readonly ReleaseNotesParser releaseNotesParser;

        [ImportingConstructor]
        public ReleaseNotesService(ReleaseNotesParser releaseNotesParser)
        {
            this.releaseNotesParser = releaseNotesParser;
        }

        public async Task<IEnumerable<ReleaseNote>> GetReleaseNotesAsync()
        {
            var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(SERVER_URI);
            httpWebRequest.Proxy = WebRequest.DefaultWebProxy;
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {

                var json = await streamReader.ReadToEndAsync();
                return releaseNotesParser.ParseJson(json);
            }
        }
    }
}
