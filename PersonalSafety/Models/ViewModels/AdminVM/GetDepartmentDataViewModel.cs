using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels.AdminVM
{
    public class GetDepartmentDataViewModel
    {
        public int Id { get; set; }
        public string DepartmentName { get; set; }
        public int AuthorityType { get; set; }
        public string AuthorityTypeName { get; set; }
        public int DistributionId { get; set; }
        public string DistributionName { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public List<string> AgentsEmails{ get; set; }
        public List<string> RescuersEmails { get; set; }
    }
}
