using PersonalSafety.Helpers;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Business
{
    public interface IClientBusiness
    {
        // /Registration
        Task<APIResponse<bool>> RegisterAsync(RegistrationViewModel request);
        APIResponse<CompleteProfileViewModel> GetEmergencyInfo(string userId);
        APIResponse<bool> CompleteProfile(string userId, CompleteProfileViewModel request);

        // /SOSRequest
        Task<APIResponse<SendSOSResponseViewModel>> SendSOSRequestAsync(string userId, SendSOSRequestViewModel request);
    }
}
