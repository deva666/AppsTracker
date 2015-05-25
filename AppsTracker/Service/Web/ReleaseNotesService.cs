using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;

namespace AppsTracker.Data.Service
{
    public sealed class ReleaseNotesService
    {
        private const string SERVER_URI = "http://www.theappstracker.com/release_notes";

        public IEnumerable<ReleaseNote> GetReleaseNotes()
        {

            return null;
        }
    }
}
