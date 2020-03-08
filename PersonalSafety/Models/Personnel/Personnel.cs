using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class Personnel
    {
        [Key, ForeignKey("ApplicationUser")]
        public string PersonnelId { get; set; }

        // Uniqueness enforced in [AppDbContext]
        [Required]
        public int AuthorityType { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
