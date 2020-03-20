using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels
{
    public class RegistrationWithFacebookViewModel
    {
        [Required]
        public string accessToken { get; set; }

        [RegularExpression(@"^[0-9]{14}$", ErrorMessage = "National ID is invalid")]
        [Required]
        public string NationalId { get; set; }

        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "Phone number is invalid")]
        [Required]
        public string PhoneNumber { get; set; }
    }
}
