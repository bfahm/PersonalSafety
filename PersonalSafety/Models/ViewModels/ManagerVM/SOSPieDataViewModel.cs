namespace PersonalSafety.Models.ViewModels.ManagerVM
{
    public class SOSPieDataViewModel
    {
        public int TotalRequests { get; set; }
        public int SolvedRequests { get; set; }
        public int AcceptedRequests { get; set; }
        public int CanceledRequests { get; set; }
        public int PendingRequests { get; set; }
    }
}
