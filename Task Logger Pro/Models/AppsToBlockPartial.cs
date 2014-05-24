using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.Models
{
    public partial class AppsToBlock
    {
        #region Wrapping Properties
        public bool MondayWrapper
        {
            get
            {
                return this.Monday;
            }
            set
            {
                this.Monday = value;
                using (var context = new AppsEntities1())
                {
                    var entry = context.AppsToBlocks.Single(a => a.AppsToBlockID == this.AppsToBlockID);
                    entry.Monday = value;
                    context.Entry(entry).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChangesAsync();
                }
            }
        }

        public bool TuesdayWrapper
        {
            get
            {
                return this.Tuesday;
            }
            set
            {
                this.Tuesday = value;
                using (var context = new AppsEntities1())
                {
                    var entry = context.AppsToBlocks.Single(a => a.AppsToBlockID == this.AppsToBlockID);
                    entry.Tuesday = value;
                    context.Entry(entry).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChangesAsync();
                }
            }
        }
        public bool WendesdayWrapper
        {
            get
            {
                return this.Wednesday;
            }
            set
            {
                this.Wednesday = value;
                using (var context = new AppsEntities1())
                {
                    var entry = context.AppsToBlocks.Single(a => a.AppsToBlockID == this.AppsToBlockID);
                    entry.Wednesday = value;
                    context.Entry(entry).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChangesAsync();
                }
            }
        }
        public bool ThursdayWrapper
        {
            get
            {
                return this.Thursday;
            }
            set
            {
                this.Thursday = value;
                using (var context = new AppsEntities1())
                {
                    var entry = context.AppsToBlocks.Single(a => a.AppsToBlockID == this.AppsToBlockID);
                    entry.Thursday = value;
                    context.Entry(entry).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChangesAsync();
                }
            }
        }
        public bool FridayWrapper
        {
            get
            {
                return this.Friday;
            }
            set
            {
                this.Friday = value;
                using (var context = new AppsEntities1())
                {
                    var entry = context.AppsToBlocks.Single(a => a.AppsToBlockID == this.AppsToBlockID);
                    entry.Friday = value;
                    context.Entry(entry).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChangesAsync();
                }
            }
        }
        public bool SaturdayWrapper
        {
            get
            {
                return this.Saturday;
            }
            set
            {
                this.Saturday = value;
                using (var context = new AppsEntities1())
                {
                    var entry = context.AppsToBlocks.Single(a => a.AppsToBlockID == this.AppsToBlockID);
                    entry.Saturday = value;
                    context.Entry(entry).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChangesAsync();
                }
            }
        }
        public bool SundayWrapper
        {
            get
            {
                return this.Sunday;
            }
            set
            {
                this.Sunday = value;
                using (var context = new AppsEntities1())
                {
                    var entry = context.AppsToBlocks.Single(a => a.AppsToBlockID == this.AppsToBlockID);
                    entry.Sunday = value;
                    context.Entry(entry).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChangesAsync();
                }
            }
        }

        public long TimeMinWrapper
        {
            get
            {
                return this.TimeMin;
            }
            set
            {
                this.TimeMin = value;
                using (var context = new AppsEntities1())
                {
                    var entry = context.AppsToBlocks.Single(a => a.AppsToBlockID == this.AppsToBlockID);
                    entry.TimeMin = value;
                    context.Entry(entry).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChangesAsync();
                }
            }
        }
        public long TimeMaxWrapper
        {
            get
            {
                return this.TimeMax;
            }
            set
            {
                this.TimeMin = value;
                using (var context = new AppsEntities1())
                {
                    var entry = context.AppsToBlocks.Single(a => a.AppsToBlockID == this.AppsToBlockID);
                    entry.TimeMax = value;
                    context.Entry(entry).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChangesAsync();
                }
            }
        }
        
        #endregion
        
        public AppsToBlock() { }
        public AppsToBlock(Uzer uzer, Aplication aplication)
        {
            this.UserID = uzer.UserID;
            this.ApplicationID = aplication.ApplicationID;
        }
    }
}
