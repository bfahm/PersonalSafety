using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels
{
    public class SendSOSRequestViewModel
    {
        public int AuthorityType { get; set; }

        [Required]
        public double Longitude { get; set; }
        [Required]
        public double Latitude { get; set; }
    }
}
