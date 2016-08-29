#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppsTracker.Data.Models
{
    public enum UsageTypes : byte
    {
        Login,
        Idle,
        Locked,
        Stopped
    }

   public class Usage 
   {
      [NotMapped]
      public TimeSpan Duration
      {
         get
         {
            return IsCurrent ? new TimeSpan(DateTime.Now.Ticks - UsageStart.Ticks) : new TimeSpan(UsageEnd.Ticks - UsageStart.Ticks);
         }
      }

      public Usage(int userID)
         : this()
      {
         UserID = userID;
      }

      public Usage(int userID, UsageTypes usageType)
         : this(userID)
      {
          UsageType = usageType;
      }

      public Usage()
      {
         UsageStart = DateTime.Now;
      }

      public Usage(Usage usage)
      {
         this.SelfUsageID = usage.SelfUsageID;
         this.IsCurrent = usage.IsCurrent;
         this.UsageEnd = usage.UsageEnd;
         this.UsageID = usage.UsageID;
         this.UsageStart = usage.UsageStart;
         this.UsageType = usage.UsageType;
         this.User = usage.User;
         this.UserID = usage.UserID;
         this.UsageType = usage.UsageType;
      }

      public DateTime GetDisplayedStart(DateTime day)
      {
         if (IsCurrent == false && UsageEnd.Date == day && UsageStart.Date == day)
            return UsageStart;

         if ((IsCurrent && UsageStart.Date < day && day <= DateTime.Now.Date))
            return day;

         if (UsageStart.Date < day && UsageEnd.Date >= day)
            return day;

         return UsageStart;
      }

      public DateTime GetDisplayedEnd(DateTime day)
      {
         if (IsCurrent && day == DateTime.Now.Date)
            return DateTime.Now;

         if ((IsCurrent && UsageStart.Date <= day && day < DateTime.Now.Date))
            return day.AddDays(1).Date;

         if (UsageStart.Date <= day && UsageEnd.Date >= day)
            return UsageEnd.Date.Date == day ? UsageEnd : day.AddDays(1).Date;

         return UsageEnd;
      }

      public long GetDisplayedTicks(DateTime day)
      {
         if (IsCurrent)
            return GetDisplayedEnd(day).Ticks - GetDisplayedStart(day).Ticks;

         if (IsCurrent == false && UsageEnd.Date == day && UsageStart.Date == day)
            return Duration.Ticks;

         if (UsageStart.Date <= day && UsageEnd.Date >= day)
            return (GetDisplayedEnd(day).Ticks - GetDisplayedStart(day).Ticks);

         return 0;
      }

      [Key]
      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
      public int UsageID { get; set; }

      [Required]
      public int UserID { get; set; }

      [Required]
      public DateTime UsageStart { get; set; }

      [Required]
      public DateTime UsageEnd { get; set; }

      [Required]
      public bool IsCurrent { get; set; }

      [Required]
      public UsageTypes UsageType { get; set; }

      public Nullable<int> SelfUsageID { get; set; }

      [ForeignKey("UserID")]
      public virtual Uzer User { get; set; }
      public virtual ICollection<Log> Logs { get; set; }
      public virtual ICollection<Usage> SelfUsages { get; set; }
      public virtual ICollection<Usage> SelfUsage { get; set; }

   }
}
