using System.Collections.Generic;

namespace AppsTracker.Data.Models
{
    public sealed class ReleaseNote
    {
        public string Version { get; private set; }
        public IEnumerable<string> Notes { get; private set; }

        public ReleaseNote(string version, IEnumerable<string> notes)
        {
            Version = version;
            Notes = notes;
        }
    }
}
