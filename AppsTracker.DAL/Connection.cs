using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AppsTracker.DAL
{
    public class Connection
    {
        public static void CheckConnection()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(typeof(Connection).Assembly.Location);

            Console.WriteLine(typeof(Connection).Assembly.Location);
            Console.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            Console.WriteLine(DateTime.Now.GetHashCode().ToString());
            var connections = config.ConnectionStrings.ConnectionStrings["AppsTracker.DAL.AppsEntities"];
            if (connections == null)
            {
                SqlCeConnectionStringBuilder sqlBuilder = new SqlCeConnectionStringBuilder
                {
                    DataSource = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "apps.sdf"),
                    Password = DateTime.Now.GetHashCode().ToString(),
                    MaxDatabaseSize = 4000
                };

                config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings("AppsTracker.DAL.AppsEntities", sqlBuilder.ConnectionString));

            }
            
            config.Save();

            var connections1 = config.ConnectionStrings.ConnectionStrings["AppsTracker.DAL.AppsEntities"];

            ToggleConfigEncryption();
        }

        static void ToggleConfigEncryption()
        {
            try
            {
                //Configuration config = ConfigurationManager.
                //    OpenExeConfiguration(exeConfigName);

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
            catch (Exception)
            {

            }
        }
    }
}
