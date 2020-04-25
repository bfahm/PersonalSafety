using PersonalSafety.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class Distribution
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int Type { get; set; }
        public string Value { get; set; }

        [ForeignKey("Distribution")]
        public int? ParentId { get; set; }
        public virtual Distribution Parent { get; set; }


        public override string ToString()
        {
            return "rule_" + Id + "_" + (DistributionTypesEnum)Type + "_" + Value;
        }
    }
}
