using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class EmergencyContact
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }

        public string Name { get; set; }
        public string PhoneNumber { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
