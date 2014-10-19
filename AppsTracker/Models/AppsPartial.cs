using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.Models
{
    public partial class AppsEntities1
    {
        //public override int SaveChanges()
        //{
        //    var changeSet = ChangeTracker.Entries<Log>();
        //    if (changeSet != null)
        //    {
        //        foreach (var log in changeSet)
        //        {
        //            if (log.Entity.DateEnded == null)
        //            {
        //                Debug.Fail("Caught log with dateend = null!");
        //                log.Entity.DateEnded = DateTime.Now;
        //                Entry(log.Entity).State = System.Data.Entity.EntityState.Modified;
        //            }

        //        }
        //    }
        //    return base.SaveChanges();
        //}
    }
}
