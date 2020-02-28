﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class SOSRequest
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }

        public int State { get; set; }
        public int AuthorityType { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
