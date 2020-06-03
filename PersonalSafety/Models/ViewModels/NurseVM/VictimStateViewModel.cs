using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels.NurseVM
{
    public class VictimStateViewModel
    {
        public string VictimEmail { get; set; }
        public List<string> SusceptibleEmails { get; set; }

        public VictimStateViewModel()
        {
            SusceptibleEmails = new List<string>();
        }
    }
}
