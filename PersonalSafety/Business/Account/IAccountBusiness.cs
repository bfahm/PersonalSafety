using PersonalSafety.Helpers;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Business.Account
{
    public interface IAccountBusiness
    {
        Task<APIResponse<string>> LoginAsync(LoginRequestViewModel request);
        Task<APIResponse<bool>> ForgotPasswordAsync(string email);
        Task<APIResponse<bool>> ResetPasswordAsync(ResetPasswordViewModel request);
        Task<APIResponse<bool>> SendConfirmMailAsync(string email);
        Task<APIResponse<bool>> ConfirmMailAsync(ConfirmMailViewModel request);
        Task<APIResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordViewModel request);
        Task<APIResponse<bool>> ValidateUserAsync(string userId, string email);
    }
}
