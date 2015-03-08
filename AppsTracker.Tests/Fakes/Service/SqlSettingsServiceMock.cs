using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Service;

namespace AppsTracker.Tests.Fakes.Service
{
    class SqlSettingsServiceMock : ISqlSettingsService
    {
        public Data.Models.Setting Settings
        {
            get { return new Data.Models.Setting(); }
        }

        public void SaveChanges(Data.Models.Setting settings)
        {
            
        }

        public Task SaveChangesAsync(Data.Models.Setting settings)
        {
            return Task.Delay(10);
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
