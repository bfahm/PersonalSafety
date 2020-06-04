using System;

namespace PersonalSafety.Models.ViewModels
{
    public class GetSOSRequestForUserViewModel
    {
        //Database
        public int RequestId { get; set; }

        //Request Data
        public int RequestStateId { get; set; }
        public string RequestStateName { get; set; }
        public double RequestLocationLongitude { get; set; }
        public double RequestLocationLatitude { get; set; }
        public DateTime RequestCreationDate { get; set; }
        public DateTime RequestLastModified { get; set; }
    }
}
