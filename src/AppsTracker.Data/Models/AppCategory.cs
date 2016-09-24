using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppsTracker.Data.Models
{
    public class AppCategory : IEntity
    {
        [NotMapped]
        public ObservableCollection<Aplication> ObservableApplications { get; set; }

        public AppCategory()
        {
            this.Applications = new HashSet<Aplication>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("AppCategoryID")]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public virtual ICollection<Aplication> Applications { get; set; }
    }
}
