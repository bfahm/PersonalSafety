using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class Event
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public int State { get; set; }

        public bool IsValidated { get; set; }
        public bool IsOnGoing { get; set; }

        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;

        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
