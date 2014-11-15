#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AppsTracker.Utils
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

#if (DEBUG && !TEST_SYMBOL)
                sqlBuilder = GetConnectionDebug();
#elif TEST_SYMBOL
                sqlBuilder = GetConnectionTest();
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

        public static void SetConectionToDebug()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var connections = config.ConnectionStrings.ConnectionStrings["AppsEntities"];
            if (connections == null)
                return;

            var connString = GetConnectionDebug().ConnectionString;

            using (SqlCeEngine engine = new SqlCeEngine(connections.ConnectionString))
            {
                engine.Compact(connString);
            }

            ConnectionStringsSection connectionStringsSection = config.ConnectionStrings;
            connectionStringsSection.ConnectionStrings.Remove("AppsEntities");

            config.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("connectionStrings");

            ConnectionStringSettings connectionStringSettings = new ConnectionStringSettings("AppsEntities", connString);
            connectionStringsSection.ConnectionStrings.Add(connectionStringSettings);

            config.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("connectionStrings");
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
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AppService");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static void ChangeDBPassword(string newPassword)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var connections = config.ConnectionStrings.ConnectionStrings["AppsEntities"];
            if (connections == null)
                return;

            SqlCeConnectionStringBuilder sqlBuilder = new SqlCeConnectionStringBuilder
            {
                DataSource = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AppService", "appsNEW.sdf"),
                MaxDatabaseSize = 4000
            };

            if (!string.IsNullOrWhiteSpace(newPassword))
                sqlBuilder.Password = newPassword;

            using (SqlCeEngine engine = new SqlCeEngine(connections.ConnectionString))
            {
                engine.Compact(sqlBuilder.ConnectionString);
            }

            ConnectionStringsSection connectionStringsSection = config.ConnectionStrings;
            connectionStringsSection.ConnectionStrings.Remove("AppsEntities");

            config.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("connectionStrings");

            ConnectionStringSettings connectionStringSettings = new ConnectionStringSettings("AppsEntities", sqlBuilder.ConnectionString);
            connectionStringsSection.ConnectionStrings.Add(connectionStringSettings);

            config.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("connectionStrings");
        }

        public static async void FlushExp()
        {
            try
            {
                if (ExpExists())
                    return;
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AppService", "sys.dll");
                byte[] bytesToWrite = Encoding.Unicode.GetBytes(DateTime.Now.Ticks.ToString());
                using (FileStream createdFile = File.Create(path, 4096, FileOptions.Asynchronous))
                {
                    await createdFile.WriteAsync(bytesToWrite, 0, bytesToWrite.Length);
                }
            }
            catch
            {
            }
        }

        public static bool ExpExists()
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AppService", "sys.dll");
                return File.Exists(path);
            }
            catch
            {
                return false;
            }
        }

        public static void DeleteExp()
        {
            try
            {
                if (!ExpExists())
                    return;
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AppService", "sys.dll");
                File.Delete(path);
            }
            catch
            {

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
