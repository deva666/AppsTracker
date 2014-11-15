#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.Data.Entity;

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
