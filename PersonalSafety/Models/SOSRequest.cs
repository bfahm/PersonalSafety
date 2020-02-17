using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class SOSRequest
    {
        public int Id { get; set; }
        
        [ForeignKey("User")]
        public int UserId { get; set; }

        public int State { get; set; }
        public int AuthorityType { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
}
