using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels
{
    public class SendSOSResponseViewModel
    {
        public int RequestId { get; set; }
        public int RequestStateId { get; set; }
        public string RequestStateName { get; set; }
    }
}
