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

        }

        public void InitializeDatabase(AppsEntities context)
        {

            if (context.Database.Exists())
                return;
            else
            {
                context.Database.Create();
            }
        }
    }
}
