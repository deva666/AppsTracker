using System;
using System.Data.SqlServerCe;
using System.IO;

namespace AppsTracker.Data.Db
{
    internal sealed class DbConnectionFactory
    {
        public static string ConnectionString { get { return GetConnectionString().ConnectionString; } }

        private static SqlCeConnectionStringBuilder GetConnectionString()
        {
            var directoryPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AppService");
            if (Directory.Exists(directoryPath) == false)
                Directory.CreateDirectory(directoryPath);

            SqlCeConnectionStringBuilder sqlBuilder = new SqlCeConnectionStringBuilder
            {
                DataSource = Path.Combine(directoryPath, "appsdb.sdf"),
                MaxDatabaseSize = 4000,
                DefaultLockTimeout = 10000
            };
            return sqlBuilder;
        }
    }
}
