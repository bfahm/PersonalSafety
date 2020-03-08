using PersonalSafety.Helpers;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Business.User
{
    public interface IClientBusiness
    {
        APIResponse<CompleteProfileViewModel> GetEmergencyInfo(string userId);
        APIResponse<bool> CompleteProfile(string userId, CompleteProfileViewModel request);
    }
}
