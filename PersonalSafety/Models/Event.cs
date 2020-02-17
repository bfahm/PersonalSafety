using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class Event
    {
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public int State { get; set; }

        public bool IsValidated { get; set; }
        public bool IsOnGoing { get; set; }


    }
}
