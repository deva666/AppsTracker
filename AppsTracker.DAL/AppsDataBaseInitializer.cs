using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Configuration;

namespace AppsTracker.DAL
{
    public class AppsDataBaseInitializer : IDatabaseInitializer<AppsEntities>
    {
        private bool _canCreate;

        public AppsDataBaseInitializer()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConnectionStringsSection section = config.GetSection("connectionStrings") as ConnectionStringsSection;
            _canCreate = section.SectionInformation.IsProtected;
        }

        public void InitializeDatabase(AppsEntities context)
        {
            if (context.Database.Exists())
                return;
            else
                if (_canCreate)
                    context.Database.Create();
        }
    }
}
