using PersonalSafety.Options;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Hubs.HubTracker;

namespace PersonalSafety.Business
{
    public interface IAgentBusiness
    {
        Task<APIResponse<List<GetSOSRequestViewModel>>> GetRelatedRequestsAsync(string userId, int? requestState);
        Task<APIResponse<bool>> RegisterRescuersAsync(string userId, RegisterRescuerViewModel accounts);
        APIResponse<DepartmentDetailsViewModel> GetDepartmentDetails(string userId);
        APIResponse<HashSet<RescuerConnectionInfo>> GetDepartmentOnlineRescuers(string userId);
        APIResponse<HashSet<RescuerConnectionInfo>> GetDepartmentDisconnectedRescuers(string userId);
        Task<APIResponse<bool>> ResetSOSRequest(int requestId);
    }
}
