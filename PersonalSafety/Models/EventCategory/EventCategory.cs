using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalSafety.Models
{
    public class EventCategory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public string ThumbnailUrl { get; set; }
    }
}
