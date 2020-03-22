using PersonalSafety.Contracts;
using PersonalSafety.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Services.Registration
{
    public interface IRegistrationService
    {
        Task<APIResponse<bool>> RegisterNewUserAsync(ApplicationUser applicationUser, string password, Client client);
        Task<APIResponse<bool>> RegisterNewUserAsync(ApplicationUser applicationUser, string password, Personnel personnel);
    }
}
