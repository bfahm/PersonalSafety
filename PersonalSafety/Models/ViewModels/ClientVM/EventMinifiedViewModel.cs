using System;

namespace PersonalSafety.Models.ViewModels.ClientVM
{
    public class EventMinifiedViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string UserName { get; set; }

        public bool IsValidated { get; set; }
        public bool IsPublicHelp { get; set; }
        public int Votes { get; set; }

        public string ThumbnailUrl { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
