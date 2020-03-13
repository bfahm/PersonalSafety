using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels
{
    public class SendSOSRequestViewModel
    {
        // Checked in the business layer to make sure it does not equal 0, so, [Required] too.
        public int AuthorityType { get; set; }
        [Required]
        public double Longitude { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public string ConnectionId { get; set; }
    }
}
