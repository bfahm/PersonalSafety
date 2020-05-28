using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels.ClientVM
{
    public class EventMinifiedViewModel
    {
        public string Title { get; set; }
        public string UserName { get; set; }

        public bool IsValidated { get; set; }
        public bool IsPublicHelp { get; set; }
        public int Votes { get; set; }

        public string ThumbnailUrl { get; set; }
    }
}
