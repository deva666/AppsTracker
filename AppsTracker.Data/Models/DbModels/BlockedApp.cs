#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppsTracker.Data.Models
{
    public class BlockedApp
    {

        public BlockedApp() { }
        public BlockedApp(int userID, int appID)
            : this()
        {
            this.Date = DateTime.Now;
            this.UserID = userID;
            this.ApplicationID = appID;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BlockedAppID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public System.DateTime Date { get; set; }

        [Required]
        public int ApplicationID { get; set; }

        [ForeignKey("ApplicationID")]
        public virtual Aplication Application { get; set; }

        [ForeignKey("UserID")]
        public virtual Uzer User { get; set; }
    }
}
