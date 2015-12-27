using System.Threading.Tasks;
using AppsTracker.Data.Models;

namespace AppsTracker.Tracking.Limits
{
    public interface IAppDurationCalc
    {
        Task<long> GetDuration(Aplication app, LimitSpan span);
    }
}