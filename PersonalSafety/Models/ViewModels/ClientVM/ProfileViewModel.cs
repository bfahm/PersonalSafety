using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PersonalSafety.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string FullName { get; set; }

        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "Phone number is invalid")]
        public string PhoneNumber { get; set; }
        
        [RegularExpression(@"^[0-9]{14}$", ErrorMessage = "National ID is invalid")]
        public string NationalId { get; set; }

        public string CurrentAddress { get; set; }
        public int BloodType { get; set; }
        public string MedicalHistoryNotes { get; set; }
        public DateTime Birthday { get; set; }

        public float UserRate { get; set; }

        public List<EmergencyContact> EmergencyContacts { get; set; }
    }
}
