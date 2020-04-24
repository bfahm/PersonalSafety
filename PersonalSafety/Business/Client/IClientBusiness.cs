using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels.AccountVM;

namespace PersonalSafety.Business
{
    public interface IClientBusiness
    {
        Task<APIResponse<bool>> RegisterAsync(RegistrationViewModel request);
        APIResponse<CompleteProfileViewModel> GetEmergencyInfo(string userId);
        APIResponse<bool> CompleteProfile(string userId, CompleteProfileViewModel request);
        Task<APIResponse<LoginResponseViewModel>> LoginWithFacebookAsync(string accessToken);
        Task<APIResponse<bool>> RegisterWithFacebookAsync(RegistrationWithFacebookViewModel request);
    }
}
