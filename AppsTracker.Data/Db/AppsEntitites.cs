#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration.Conventions;
using AppsTracker.Data.Models;

namespace AppsTracker.Data.Db
{
    internal sealed class AppsEntities : DbContext
    {
        private static string connectionString = DbConnectionFactory.ConnectionString;

        public static string ConnectionString { get { return connectionString; } }

        public AppsEntities()
            : base(connectionString)
        {
            Database.SetInitializer<AppsEntities>(new DropCreateDatabaseIfModelChanges<AppsEntities>());
#if DEBUG
            Database.Log = FlushSql;
#endif
        }

        private void FlushSql(string s)
        {
           // System.Diagnostics.Debug.WriteLine(s);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Entity<Aplication>()
                .HasMany(a => a.Categories)
                .WithMany(c => c.Applications)
                .Map(m =>
                {
                    m.MapLeftKey("ApplicationID");
                    m.MapRightKey("AppCategoryID");
                    m.ToTable("ApplicationCategories");
                });

            modelBuilder.Entity<Aplication>()
                .Property(a => a.Name)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("UQ_Aplication_Name_UserID", 0) { IsUnique = true }));

            modelBuilder.Entity<Aplication>()
                .Property(a => a.UserID)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("UQ_Aplication_Name_UserID", 1) { IsUnique = true }));

            modelBuilder.Entity<Window>()
                .Property(w => w.ApplicationID)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("UQ_Window_Title_ApplicationID", 0) { IsUnique = true }));

            modelBuilder.Entity<Window>()
               .Property(w => w.Title)
               .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("UQ_Window_Title_ApplicationID", 1) { IsUnique = true }));

            modelBuilder.Entity<Log>()
                .Property(l => l.WindowID)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Log_WindowID")));

            modelBuilder.Entity<Log>()
                .Property(l => l.DateCreated)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Log_DateCreated")));

            modelBuilder.Entity<Log>()
                .Property(l => l.DateEnded)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Log_DateEnded")));

            modelBuilder.Entity<AppLimit>()
                .Property(l => l.ApplicationID)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_AppLimit_ApplicationID")));

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Aplication> Applications { get; set; }
        public DbSet<AppCategory> AppCategories { get; set; }
        public DbSet<AppLimit> AppLimits { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Screenshot> Screenshots { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Uzer> Users { get; set; }
        public DbSet<Window> Windows { get; set; }
        public DbSet<Usage> Usages { get; set; }
    }
}
