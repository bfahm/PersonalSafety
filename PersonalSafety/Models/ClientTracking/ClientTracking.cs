using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalSafety.Models
{
    public class ClientTracking
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ApplicationUser")]
        public string ClientId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        
        public DateTime Time { get; set; }
    }
}
