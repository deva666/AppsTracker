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
using Microsoft.Win32;

namespace AppsTracker.Data.Service
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

        public SqlSettingsService()
        {
            CreateSettings();
            ReadSettingsFromRegistry();
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

        private void ReadSettingsFromRegistry()
        {           
            bool? exists = RegistryEntryExists();
            if (exists == null && settings.RunAtStartup)
                settings.RunAtStartup = false;
            else if (exists.HasValue && exists.Value && !settings.RunAtStartup)
                settings.RunAtStartup = true;
            else if (exists.HasValue && !exists.Value && settings.RunAtStartup)
                settings.RunAtStartup = false;

            SaveChanges(settings);
        }

        private bool? RegistryEntryExists()
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (key.GetValue("app service") == null)
                    return false;
                return true;
            }
            catch
            {
                return null;
            }
        }


        private void NotifyPropertyChanged()
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("Settings"));
        }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
