using System;

namespace PersonalSafety.Models.ViewModels.ClientVM
{
    public class EventDetailedViewModel
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public int? EventCategoryId { get; set; }
        public string EventCategoryName { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public bool IsValidated { get; set; }
        public bool IsPublicHelp { get; set; }
        public int Votes { get; set; }

        public string ThumbnailUrl { get; set; }

        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
    }
}
