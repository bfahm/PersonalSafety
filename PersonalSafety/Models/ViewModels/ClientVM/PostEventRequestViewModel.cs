using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels.ClientVM
{
    public class PostEventRequestViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int? EventCategoryId { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public bool IsPublicHelp { get; set; }
        public IFormFile Thumbnail { get; set; }
    }
}
