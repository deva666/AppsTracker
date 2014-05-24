using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppsTracker.Models.EntityModels
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
