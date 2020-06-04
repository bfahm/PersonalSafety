using PersonalSafety.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Models.ViewModels;

namespace PersonalSafety.Business
{
    public interface ISOSBusiness
    {
        Task<APIResponse<SendSOSResponseViewModel>> SendSOSRequestAsync(string userId, SendSOSRequestViewModel request);
        APIResponse<bool> AcceptSOSRequest(int requestId, string rescuerEmail);
        Task<APIResponse<bool>> CancelSOSRequestAsync(int requestId, string clientUserId, bool notifyClient = true);
        Task<APIResponse<bool>> CancelPendingRequestsAsync(string userId, bool notifyClient = true);
        Task<APIResponse<bool>> SolveSOSRequestAsync(int requestId, string rescuerUserId);
        APIResponse<bool> ResetSOSRequest(int requestId);

        APIResponse<bool> RateRescuerAsync(string userId, int requestId, int rate);

        APIResponse<List<GetSOSRequestForUserViewModel>> GetSOSRequestsHistory(string userId);
    }
}
