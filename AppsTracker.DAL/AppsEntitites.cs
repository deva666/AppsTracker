#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

using AppsTracker.Models.EntityModels;

namespace AppsTracker.DAL
{
    public class AppsEntities : DbContext
    {
        private static string _connectionString = GetConnectionString();

        public static string ConnectionString { get { return _connectionString; } }

        private static string GetConnectionString()
        {
            var connections = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).ConnectionStrings.ConnectionStrings["AppsEntities"];
            if (connections != null)
                return connections.ConnectionString;
            else
                throw new ApplicationException("Connection string not found");
        }

        public AppsEntities()
            : base(_connectionString)
        {
#if DEBUG
            Database.Log = FlushSql;
#endif
        }

        private void FlushSql(string s)
        {
            // Debug.WriteLine(s);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Aplication> Applications { get; set; }
        public DbSet<AppsToBlock> AppsToBlocks { get; set; }
        public DbSet<BlockedApp> BlockedApps { get; set; }
        public DbSet<FileLog> FileLogs { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Screenshot> Screenshots { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Uzer> Users { get; set; }
        public DbSet<Window> Windows { get; set; }
        public DbSet<Usage> Usages { get; set; }
        public DbSet<UsageType> UsageTypes { get; set; }
    }
}
