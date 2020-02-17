using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class User
    {
        // Automatically converted to primary key
        public int Id { get; set; }
        
        // Should be Unique
        [Required]
        public string NationalId { get; set; }
        
        [Required]
        public string FullName { get; set; }
        
        // Should be Unique
        [Required]
        public string PhoneNumber { get; set; }

        public DateTime Birthday { get; set; }

        // Should be Unique
        // This column should be dropped when ASP Identity is set up
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Invalid email format")]
        [Required]
        public string Email { get; set; }

        public int BloodType { get; set; }

        public string MedicalHistoryNotes { get; set; }

        public string CurrentAddress { get; set; }

        [ForeignKey("Event")]
        public int CurrentOngoingEvent { get; set; }

        [ForeignKey("Event")]
        public int CurrentInvolvement { get; set; }
    }
}
