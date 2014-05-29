using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppsTracker.Models.EntityModels
{
    public class FileLog
    {

        public FileLog()
        {
            this.Date = DateTime.Now;
            this.NewPath = "";
        }

        public FileLog(string oldPath, string eventType, int userID)
            : this()
        {
            this.Path = oldPath;
            this.Event = eventType;
            this.UserID = userID;
        }

        public FileLog(string oldPath, string eventType, string newPath, int userID)
            : this(oldPath, eventType, userID)
        {
            this.NewPath = newPath;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FileLogID { get; set; }

        [Required]
        [StringLength(360)]
        public string Path { get; set; }

        [StringLength(360)]
        public string NewPath { get; set; }

        [Required]
        [StringLength(60)]
        public string Event { get; set; }

        [Required]
        public System.DateTime Date { get; set; }
        
        [Required]
        public int UserID { get; set; }
        
        [ForeignKey("UserID")]
        public virtual Uzer User { get; set; }
    }
}
