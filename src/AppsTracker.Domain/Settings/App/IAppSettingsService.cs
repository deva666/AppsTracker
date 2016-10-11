using System.ComponentModel;
using System.Threading.Tasks;
using AppsTracker.Data.Models;

namespace AppsTracker.Domain.Settings
{
    public interface IAppSettingsService : INotifyPropertyChanged
    {
        Setting Settings { get; }

        void SaveChanges(Setting settings);

        Task SaveChangesAsync(Setting settings);
    }
}