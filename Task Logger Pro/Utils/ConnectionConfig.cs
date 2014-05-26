using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.Utils
{
    public static class ConnectionConfig
    {
        public static void CheckConnection()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var connections = config.ConnectionStrings.ConnectionStrings["AppsEntities"];

            ConnectionStringsSection section =
                config.GetSection("connectionStrings")
                as ConnectionStringsSection;

            if (connections == null)
            {

                SqlCeConnectionStringBuilder sqlBuilder = new SqlCeConnectionStringBuilder
               {
                   DataSource = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AppService", "apps.sdf"),
                   Password = DateTime.Now.Ticks.GetHashCode().ToString(),
                   MaxDatabaseSize = 4000
                   
               };

                ConnectionStringsSection connectionStringsSection = config.ConnectionStrings;

                ConnectionStringSettings connectionStringSettings = new ConnectionStringSettings("AppsEntities", sqlBuilder.ConnectionString);
                connectionStringsSection.ConnectionStrings.Add(connectionStringSettings);

                config.Save(ConfigurationSaveMode.Full, true);
                ConfigurationManager.RefreshSection("connectionStrings");

            }
            else if (connections != null && !section.SectionInformation.IsProtected)
            {
                throw new InvalidOperationException("Can't create database!");
            }
        }

        public static void ToggleConfigEncryption()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            ConnectionStringsSection section =
                config.GetSection("connectionStrings")
                as ConnectionStringsSection;

            if (!section.SectionInformation.IsProtected)
            {
                section.SectionInformation.ProtectSection(
                      "DataProtectionConfigurationProvider");

                config.Save(ConfigurationSaveMode.Full, true);
                ConfigurationManager.RefreshSection("connectionStrings");
            }
        }

        public static void SaveConfig()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            ConnectionStringsSection section =
                config.GetSection("connectionStrings")
                as ConnectionStringsSection;

            if (!section.SectionInformation.IsProtected)
            {
                section.SectionInformation.ProtectSection(
                      "DataProtectionConfigurationProvider");

                config.Save();
            }
        }
    }
}
