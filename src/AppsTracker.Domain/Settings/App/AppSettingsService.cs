﻿#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Model;
using Microsoft.Win32;

namespace AppsTracker.Domain.Settings
{
    [Export(typeof(IAppSettingsService))]
    public sealed class AppSettingsService : ObservableObject, IAppSettingsService
    {
        private readonly IRepository repository;

        private Setting settings;
        public Setting Settings
        {
            get
            {
                var clone = settings.Clone();
                return clone;
            }
        }

        [ImportingConstructor]
        public AppSettingsService(IRepository repository)
        {
            this.repository = repository;
            CreateSettings();
            ReadSettingsFromRegistry();
        }

        private void CreateSettings()
        {
            var allSettings = repository.Get<Setting>();
            if (allSettings.Count() == 0)
            {
                settings = new Setting(true);
                repository.SaveNewEntity(settings);
            }
            else
            {
                settings = allSettings.First();
            }
        }

        public void SaveChanges(Setting settings)
        {
            repository.SaveModifiedEntity(settings);
            this.settings = settings;

            PropertyChanging("Settings");
        }

        public async Task SaveChangesAsync(Setting settings)
        {
            await repository.SaveModifiedEntityAsync(settings);
            this.settings = settings;

            PropertyChanging("Settings");
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
    }
}
