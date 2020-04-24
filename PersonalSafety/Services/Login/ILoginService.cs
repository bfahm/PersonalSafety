using PersonalSafety.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels.AccountVM;

namespace PersonalSafety.Services
{
    public interface ILoginService
    {
        Task<AuthenticationDetailsViewModel> GenerateAuthenticationDetailsAsync(ApplicationUser user);
        APIResponseData ValidateRefreshToken(string token, string refreshToken, out ClaimsPrincipal validatedToken);
        ClaimsPrincipal GetPrincipalFromToken(string token);
    }
}
