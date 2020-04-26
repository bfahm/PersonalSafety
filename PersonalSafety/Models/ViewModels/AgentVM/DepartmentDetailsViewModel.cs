using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels
{
    public class DepartmentDetailsViewModel
    {
        public int DepartmentId { get; set; }
        public double DepartmentLongitude { get; set; }
        public double DepartmentLatitude { get; set; }
        public int DistributionId { get; set; }
        public string DistributionName { get; set; }
        public int AuthorityTypeId { get; set; }
        public string AuthorityTypeName { get; set; }
        public List<string> AgentsEmails { get; set; }
        public List<string> RescuerEmails { get; set; }
    }
}
