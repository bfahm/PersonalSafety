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
        Task<APIResponse<bool>> UpdateSOSRequestAsync(int requestId, int newStatus, string issuerId, string rescuerEmail = null);
        Task<APIResponse<bool>> CancelPendingRequests(string userId);
    }
}
