using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;

namespace AppsTracker.Data.Service
{
    public sealed class ReleaseNotesService
    {
        private const string SERVER_URI = "http://www.theappstracker.com/release_notes";

        public async Task< IEnumerable<ReleaseNote>> GetReleaseNotes()
        {
            var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(SERVER_URI);
            httpWebRequest.Proxy = WebRequest.DefaultWebProxy;
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                //return await streamReader.ReadToEndAsync();
            }
            return null;
        }
    }
}
