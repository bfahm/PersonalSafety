using PersonalSafety.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public int State { get; set; } = (int)StatesTypesEnum.Pending;
        public int AuthorityType { get; set; }

        [Required]
        public double Longitude { get; set; }
        [Required]
        public double Latitude { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
