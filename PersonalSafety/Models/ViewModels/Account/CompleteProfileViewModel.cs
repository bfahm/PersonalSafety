using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels
{
    public class CompleteProfileViewModel
    {
        public string CurrentAddress { get; set; }
        public int BloodType { get; set; }
        public string MedicalHistoryNotes { get; set; }
        public List<EmergencyContact> EmergencyContacts { get; set; }
    }
}
