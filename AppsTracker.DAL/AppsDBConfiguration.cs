#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServerCompact;

namespace AppsTracker.DAL
{
    class AppsDBConfiguration : DbConfiguration
    {
        public AppsDBConfiguration()
        {
            SetProviderServices("System.Data.SqlServerCe.4.0", SqlCeProviderServices.Instance);
            SetDefaultConnectionFactory(new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0", "", AppsEntities.ConnectionString));
        }
    }
}
