using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Common.Utils;
using AppsTracker.Data.Models;
using Newtonsoft.Json.Linq;

namespace AppsTracker.Service.Web
{
    public sealed class ReleaseNotesParser
    {
        public IEnumerable<ReleaseNote> ParseJson(string json)
        {
            Ensure.NotNull(json, "json");

            var jArray = JArray.Parse(json);
            var releaseNotes = new List<ReleaseNote>();
            foreach (var item in jArray)
            {
                var version = (string)item["version_number"];
                var notes = item.SelectToken("release_notes").Where(s => s != null).Select(s => (string)s);
                releaseNotes.Add(new ReleaseNote(version, notes));
            }            

            return releaseNotes;
        }
    }
}
