using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Repository;

namespace AppsTracker.Domain.Screenshots
{
    public sealed class ScreenshotsService
    {
        private readonly IRepository repository;



        public async Task DeleteScreenshotsAsync(IEnumerable<ScreenshotModel> screenshots)
        {
            await repository.DeleteScreenshotsById(screenshots.Select(s => s.ScreenshotId));
        }
    }
}
