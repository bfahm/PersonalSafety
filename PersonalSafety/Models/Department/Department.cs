using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Contracts.Enums;

namespace PersonalSafety.Models
{
    public class Department
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int AuthorityType { get; set; }

        public int City { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public override string ToString()
        {
            return "dpt_" + Id + "_" + (CitiesEnum) City + "_" + (AuthorityTypesEnum) AuthorityType;
        }
    }
}
