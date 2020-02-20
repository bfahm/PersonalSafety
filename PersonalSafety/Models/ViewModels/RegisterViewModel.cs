using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels
{
    public class RegisterViewModel
    {
        //TODO: Enable in the future, just trying now with email and password only
        //[Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        //TODO: Enable in the future, just trying now with email and password only
        //[RegularExpression(@"^[0-9]{14}$", ErrorMessage = "National ID is invalid")]
        //[Required]
        public string NationalId { get; set; }

        //TODO: Enable in the future, just trying now with email and password only
        //[RegularExpression(@"^[0-9]{11}$", ErrorMessage = "Phone number is invalid")]
        //[Required]
        public string PhoneNumber { get; set; }
    }
}
