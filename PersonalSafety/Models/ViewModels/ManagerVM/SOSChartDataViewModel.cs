using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels.ManagerVM
{
    public class SOSChartDataViewModel
    {
        public int TotalRequests { get; set; }
        public int SolvedRequests { get; set; }
        public int AcceptedRequests { get; set; }
        public int CanceledRequests { get; set; }
        public int PendingRequests { get; set; }
    }
}
