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
        APIResponse<IEnumerable<EmergencyContact>> GetEmergencyContacts(string userId);
        Task<APIResponse<bool>> CompleteProfileAsync(string userId, CompleteProfileViewModel request);
    }
}
