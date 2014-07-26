using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.SqlServerCompact;
using System.Data.Entity.Infrastructure;

namespace AppsTracker.DAL
{
    class AppsDBConfiguration : DbConfiguration
    {
        public AppsDBConfiguration()
        {
            SetProviderServices("System.Data.SqlServerCe.4.0", SqlCeProviderServices.Instance);
            SetDefaultConnectionFactory(new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0", "", AppsEntities.GetConnectionString()));
        }
    }
}
