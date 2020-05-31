using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels.ClientVM
{
    public class PostEventResponseViewModel
    {
        public int EventId { get; set; }
        public int AssignedCityId { get; set; }
        public string AssignedCityName { get; set; }
    }
}
