using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Test
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App() { }
        public static void CheckConnection()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var connections = config.ConnectionStrings.ConnectionStrings["AppsEntities"];
            if (connections == null)
            {
                SqlCeConnectionStringBuilder sqlBuilder = new SqlCeConnectionStringBuilder
                {
                    DataSource = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AppService" , "apps.sdf"),
                    Password = DateTime.Now.Ticks.GetHashCode().ToString(),
                    MaxDatabaseSize = 4000
                };

                config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings("AppsEntities", sqlBuilder.ConnectionString));

                config.Save();
            }

            var connections1 = config.ConnectionStrings.ConnectionStrings["AppsEntities"];


            ToggleConfigEncryption();
        }

        static void ToggleConfigEncryption()
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                ConnectionStringsSection section =
                    config.GetSection("connectionStrings")
                    as ConnectionStringsSection;
            
                config.Save();
                if (!section.SectionInformation.IsProtected)
                {
                    section.SectionInformation.ProtectSection(
                          "DataProtectionConfigurationProvider");

                    config.Save();
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
