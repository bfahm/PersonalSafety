using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels
{
    public class ModifyManagerViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public int DistributionId { get; set; }
    }
}
