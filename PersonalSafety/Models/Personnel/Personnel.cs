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
        public virtual ApplicationUser ApplicationUser { get; set; }

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; }

        public bool IsRescuer { get; set; } // False means he is an agent

        public float Rate { get; set; }
    }
}
