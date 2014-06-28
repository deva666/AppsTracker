using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

            ConnectionStringsSection section = config.GetSection("connectionStrings") as ConnectionStringsSection;

            if (connections == null)
            {

                SqlCeConnectionStringBuilder sqlBuilder;

#if DEBUG
                sqlBuilder = GetConnectionDebug();
#else
                sqlBuilder = GetConnectionRelease();
#endif

                try
                {
                    CreateDBFolder();
                }
                catch (Exception ex)
                {
                    throw new IOException("Can't create DB folder", ex);
                }

                ConnectionStringsSection connectionStringsSection = config.ConnectionStrings;

                ConnectionStringSettings connectionStringSettings = new ConnectionStringSettings("AppsEntities", sqlBuilder.ConnectionString);
                connectionStringsSection.ConnectionStrings.Add(connectionStringSettings);

                config.Save(ConfigurationSaveMode.Full, true);

                ConfigurationManager.RefreshSection("connectionStrings");

            }
            else if (connections != null && !section.SectionInformation.IsProtected)
            {
                throw new System.Security.SecurityException("Database creation forbidden!");
            }
        }

        private static SqlCeConnectionStringBuilder GetConnectionRelease()
        {
            string pass;
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var str = DateTime.Now.Ticks.ToString();
                byte[] bytes = new byte[str.Length * sizeof(char)];
                System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
                var hash = sha1.ComputeHash(bytes);
                pass = Convert.ToBase64String(hash);
            }

            SqlCeConnectionStringBuilder sqlBuilder = new SqlCeConnectionStringBuilder
            {
                DataSource = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AppService", "apps.sdf"),
                Password = pass,
                MaxDatabaseSize = 4000

            };
            return sqlBuilder;
        }

        private static SqlCeConnectionStringBuilder GetConnectionDebug()
        {
            SqlCeConnectionStringBuilder sqlBuilder = new SqlCeConnectionStringBuilder
            {
                DataSource = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AppService", "appsRELEASE.sdf"),
                MaxDatabaseSize = 4000

            };
            return sqlBuilder;
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

        private static void CreateDBFolder()
        {
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AppService")))
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AppService"));
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
