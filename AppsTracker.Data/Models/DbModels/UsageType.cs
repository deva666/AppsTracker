#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

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

    public class UsageType
    {
        public UsageType()
        {
            this.Usages = new HashSet<Usage>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UsageTypeID { get; set; }

        [Required]
        [StringLength(25)]
        public string UType { get; set; }

        public virtual ICollection<Usage> Usages { get; set; }
    }
}
