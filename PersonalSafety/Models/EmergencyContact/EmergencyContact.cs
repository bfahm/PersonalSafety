﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class EmergencyContact
    {
        [JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [JsonIgnore]
        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }

        public string Name { get; set; }

        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "Phone number is invalid")]
        public string PhoneNumber { get; set; }

        [JsonIgnore]
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
