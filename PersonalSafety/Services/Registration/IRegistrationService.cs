using PersonalSafety.Contracts;
using PersonalSafety.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static PersonalSafety.Services.RegistrationService;

namespace PersonalSafety.Services
{
    public interface IRegistrationService
    {
        Task<APIResponse<bool>> RegisterClientAsync(ApplicationUser applicationUser, string password, Client client);
        Task<APIResponse<bool>> RegisterWorkingEntityAsync(ApplicationUser applicationUser, string password, EntityAdder entityAdder, string[] roles, Claim[] claims);
    }
}
