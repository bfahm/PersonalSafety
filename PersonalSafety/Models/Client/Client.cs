using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalSafety.Models
{
    public class Client
    {
        [Key, ForeignKey("ApplicationUser")]
        public string ClientId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        public int RateCount { get; set; }
        public float RateAverage { get; set; }

        // Uniqueness enforced in [AppDbContext]
        [Required]
        public string NationalId { get; set; }

        public DateTime Birthday { get; set; }

        public int BloodType { get; set; }

        public string MedicalHistoryNotes { get; set; }

        public string CurrentAddress { get; set; }

        [ForeignKey("LastKnownCity")]
        public int? LastKnownCityId { get; set; }
        public virtual Distribution LastKnownCity { get; set; }

        public string DeviceRegistrationKey { get; set; }
    }
}
