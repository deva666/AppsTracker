using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.ComponentModel.DataAnnotations;
using AppsTracker.Models.EntityModels;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Configuration;

namespace AppsTracker.DAL
{
    public class AppsEntities : DbContext
    {
        public static string GetConnectionString()
        {
            var connections = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).ConnectionStrings.ConnectionStrings["AppsEntities"];
            if (connections != null)
                return connections.ConnectionString;
            else
                throw new ApplicationException("Connection string not found");
        }

        public AppsEntities()
            : base(GetConnectionString())
        {
            
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
