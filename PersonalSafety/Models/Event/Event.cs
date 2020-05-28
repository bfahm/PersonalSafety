using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalSafety.Models
{
    public class Event
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        [ForeignKey("EventCategory")]
        public int? EventCategoryId { get; set; }
        public virtual EventCategory EventCategory { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public int State { get; set; }

        public bool IsValidated { get; set; }
        public bool IsPublicHelp { get; set; }
        public int Votes { get; set; }

        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
    }
}
