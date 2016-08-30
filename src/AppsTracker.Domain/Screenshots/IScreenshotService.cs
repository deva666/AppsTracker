using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppsTracker.Domain.Screenshots
{
    public interface IScreenshotService : IUseCaseAsync<ScreenshotModel>
    {
        Task DeleteScreenshotsAsync(IEnumerable<Image> screenshots);
    }
}