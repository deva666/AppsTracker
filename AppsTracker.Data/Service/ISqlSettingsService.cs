using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;

namespace AppsTracker.Data.Service
{
    public interface ISqlSettingsService : IBaseService, INotifyPropertyChanged
    {
        Setting Settings { get; }
        void SaveChanges(Setting settings);
        Task SaveChangesAsync(Setting settings);
    }
}
