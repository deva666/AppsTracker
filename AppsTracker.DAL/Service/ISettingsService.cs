using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Models.EntityModels;

namespace AppsTracker.DAL.Service
{
    public interface ISettingsService : IBaseService, INotifyPropertyChanged
    {
        Setting Settings { get; }
        void SaveChanges(Setting settings);
        Task SaveChangesAsync(Setting settings);
    }
}
