using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels.AccountVM
{
    public class AccountBasicInfoViewModel
    {
        public string FullName { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string AuthorityTypeName { get; set; }
    }
}
