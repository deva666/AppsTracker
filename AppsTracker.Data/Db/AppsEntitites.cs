#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using AppsTracker.Data.Models;

namespace AppsTracker.Data.Db
{
    public sealed class AppsEntities : DbContext
    {
        private static string _connectionString = DbConnectionFactory.ConnectionString;

        public static string ConnectionString { get { return _connectionString; } }

        public AppsEntities()
            : base(_connectionString)
        {
            Database.SetInitializer<AppsEntities>(new DropCreateDatabaseIfModelChanges<AppsEntities>());
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

            modelBuilder.Entity<Aplication>().
                HasMany(a => a.Categories)
                .WithMany(c => c.Applications)
                .Map(m =>
                {
                    m.MapLeftKey("ApplicationID");
                    m.MapRightKey("AppCategoryID");
                    m.ToTable("ApplicationCategories");
                });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Aplication> Applications { get; set; }
        public DbSet<AppCategory> AppCategories { get; set; }
        public DbSet<AppLimit> AppLimits { get; set; }
        public DbSet<BlockedApp> BlockedApps { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Screenshot> Screenshots { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Uzer> Users { get; set; }
        public DbSet<Window> Windows { get; set; }
        public DbSet<Usage> Usages { get; set; }
    }
}
