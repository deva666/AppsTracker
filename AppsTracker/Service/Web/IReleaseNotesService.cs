using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
namespace AppsTracker.Service.Web
{
    interface IReleaseNotesService
    {
        Task<IEnumerable<ReleaseNote>> GetReleaseNotesAsync();
    }
}
