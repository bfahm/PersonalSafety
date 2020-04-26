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
        Task<APIResponse<List<GetSOSRequestViewModel>>> GetRequestsByStateAsync(string userId, int? requestState);
        Task<APIResponse<bool>> RegisterRescuersAsync(string userId, RegisterRescuerViewModel accounts);
        APIResponse<DepartmentDetailsViewModel> GetDepartmentDetails(string userId);
        APIResponse<List<RescuerConnectionInfo>> GetDepartmentOnlineRescuers(string userId);
        APIResponse<List<RescuerConnectionInfo>> GetDepartmentDisconnectedRescuers(string userId);
    }
}
