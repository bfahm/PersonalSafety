using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels.AdminVM
{
    public class GetDepartmentDataViewModel
    {
        public int Id { get; set; }
        public int AuthorityType { get; set; }
        public string AuthorityTypeName { get; set; }
        public int City { get; set; }
        public string CityName { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public List<string> AgentsEmails{ get; set; }
        public List<string> RescuersEmails { get; set; }
    }
}
