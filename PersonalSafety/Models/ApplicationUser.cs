using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Uniqueness enforced in [AppDbContext]
        [Required]
        public string NationalId { get; set; }
        
        [Required]
        public string FullName { get; set; }
        
        public DateTime Birthday { get; set; }

        public int BloodType { get; set; }

        public string MedicalHistoryNotes { get; set; }

        public string CurrentAddress { get; set; }

        [ForeignKey("Event")]
        public int CurrentOngoingEvent { get; set; }

        [ForeignKey("Event")]
        public int CurrentInvolvement { get; set; }
    }
}
