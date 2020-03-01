using PersonalSafety.Helpers;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Business.User
{
    public interface IUserBusiness
    {
        Task<APIResponse<CompleteProfileViewModel>> GetEmergencyInfo(string userId);
        Task<APIResponse<bool>> CompleteProfileAsync(string userId, CompleteProfileViewModel request);
    }
}
