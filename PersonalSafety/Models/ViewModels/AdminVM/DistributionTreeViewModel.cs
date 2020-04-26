using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels.AdminVM
{
    public class DistributionTreeViewModel
    {
        public int Id { get; set; }

        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public string Value { get; set; }
        public List<DistributionTreeViewModel> Children { get; set; }

        public DistributionTreeViewModel()
        {
            Children = new List<DistributionTreeViewModel>();
        }
    }
}
