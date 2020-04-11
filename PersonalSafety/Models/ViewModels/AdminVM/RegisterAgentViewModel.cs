using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels
{
    public class RegisterAgentViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public int ExistingDepartmentId { get; set; }

        public int AuthorityType { get; set; }

        public int DepartmentCity { get; set; }

        public double DepartmentLongitude { get; set; }

        public double DepartmentLatitude { get; set; }
    }
}
