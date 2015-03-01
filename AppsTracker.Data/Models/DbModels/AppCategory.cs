using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppsTracker.Data.Models
{
    public class AppCategory
    {
        [NotMapped]
        public bool IsSelected { get; set; }

        public AppCategory()
        {
            this.Applications = new HashSet<Aplication>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AppCategoryID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public virtual ICollection<Aplication> Applications { get; set; }
    }
}
