using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppsTracker.Data.Db;
using AppsTracker.Data.Models;

namespace AppsTracker.Data.Service
{
    public sealed class SqlSettingsService : ISqlSettingsService
    {
        private static Lazy<SqlSettingsService> instance = new Lazy<SqlSettingsService>(() => new SqlSettingsService());

        public static SqlSettingsService Instance
        {
            get { return instance.Value; }
        }

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
