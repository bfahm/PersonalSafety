using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels
{
    public class NewEventCategoryViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile Thumbnail { get; set; }
    }
}
