#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using AppsTracker.Data.Db;
using AppsTracker.Data.Models;

namespace AppsTracker.Service
{
    [Export(typeof(ISqlSettingsService))]
    public sealed class SqlSettingsService : ISqlSettingsService
    {
        private Setting settings;
        public Setting Settings
        {
            get
            {
                var clone = settings.Clone();
                return clone;
            }
        }

        private SqlSettingsService()
        {
            CreateSettings();
        }

        private void CreateSettings()
        {
            using (var context = new AppsEntities())
            {
                if (context.Settings.Count() == 0)
                {
                    settings = new Setting(true);
                    context.Settings.Add(settings);
                    context.SaveChanges();
                }
                else
                {
                    settings = context.Settings.FirstOrDefault();
                }
            }
        }

        public void SaveChanges(Setting settings)
        {
            using (var context = new AppsEntities())
            {
                context.Entry<Setting>(settings).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
            }
            this.settings = settings;

            NotifyPropertyChanged();
        }

        public async Task SaveChangesAsync(Setting settings)
        {
            using (var context = new AppsEntities())
            {
                context.Entry<Setting>(settings).State = System.Data.Entity.EntityState.Modified;
                await context.SaveChangesAsync();
            }
            this.settings = settings;

            NotifyPropertyChanged();
        }


        private void NotifyPropertyChanged()
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("Settings"));
        }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
