using PersonalSafety.Options;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Contracts;

namespace PersonalSafety.Business
{
    public interface IAgentBusiness
    {
        Task<APIResponse<List<GetSOSRequestViewModel>>> GetRelatedRequestsAsync(string userId, int? requestState);
        Task<APIResponse<bool>> RegisterRescuersAsync(string userId, RegisterRescuerViewModel accounts);
    }
}
