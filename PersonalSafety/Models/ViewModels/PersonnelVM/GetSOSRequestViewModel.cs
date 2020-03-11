using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels
{
    public class GetSOSRequestViewModel
    {
        //User Data
        public string UserEmail { get; set; }
        public string UserPhoneNumber { get; set; }
        public string UserNationalId { get; set; }
        public int UserAge { get; set; }
        public int UserBloodType { get; set; }
        public string UserMedicalHistoryNotes { get; set; }
        public string UserSavedAddress { get; set; }

        //Request Data
        public int RequestStateId { get; set; }
        public string RequestStateName { get; set; }
        public double RequestLocationLongitude { get; set; }
        public double RequestLocationLatitude { get; set; }
    }
}
