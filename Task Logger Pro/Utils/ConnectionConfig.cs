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
                // Add the new element to the section.
                connectionStringsSection.ConnectionStrings.Add(connectionStringSettings);

                // Save the configuration file.
                config.Save(ConfigurationSaveMode.Full, true);
                ConfigurationManager.RefreshSection("connectionStrings");
                // This is this needed. Otherwise the connection string does not show up in
                // ConfigurationManager
                //ConfigurationManager.RefreshSection("connectionStrings");


                //SqlCeConnectionStringBuilder sqlBuilder = new SqlCeConnectionStringBuilder
                //{
                //    DataSource = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AppService", "apps.sdf"),
                //    Password = DateTime.Now.Ticks.GetHashCode().ToString(),
                //    MaxDatabaseSize = 4000
                //};

                //config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings("AppsEntities", sqlBuilder.ConnectionString));

                //config.Save(ConfigurationSaveMode.Full, true);
                ToggleConfigEncryption();
            }
        }

        static void ToggleConfigEncryption()
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
