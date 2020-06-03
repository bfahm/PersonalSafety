using System;

namespace PersonalSafety.Models.ViewModels
{
    public class GetUserDataViewModel
    {
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public string UserPhoneNumber { get; set; }
        public string UserNationalId { get; set; }
        public int UserAge { get; set; }
        public int UserBloodTypeId { get; set; }
        public string UserBloodTypeName { get; set; }
        public string UserMedicalHistoryNotes { get; set; }
        public string UserSavedAddress { get; set; }
    }
}
