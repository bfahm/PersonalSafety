using PersonalSafety.Helpers;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Services
{
    public interface IAccountBusiness
    {
        Task<APIResponse<string>> RegisterAsync(RegistrationRequestViewModel request);
        Task<APIResponse<string>> LoginAsync(LoginRequestViewModel request);
    }
}
