using PersonalSafety.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Services
{
    public interface IJwtAuthService
    {
        Task<string> GenerateAuthenticationTokenAsync(ApplicationUser user);
    }
}
