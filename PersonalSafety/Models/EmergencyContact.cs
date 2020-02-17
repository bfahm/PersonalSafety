using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class EmergencyContact
    {
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }

        public string Name { get; set; }
        public string PhoneNumber { get; set; }
    }
}
