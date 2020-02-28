using PersonalSafety.Helpers;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Business
{
    public interface IAccountBusiness
    {
        Task<APIResponse<bool>> RegisterAsync(RegistrationViewModel request);
        Task<APIResponse<string>> LoginAsync(LoginRequestViewModel request);
        Task<APIResponse<bool>> ForgotPasswordAsync(string email);
        Task<APIResponse<bool>> ResetPasswordAsync(ResetPasswordViewModel request);
        Task<APIResponse<bool>> SendConfirmMailAsync(string email);
        Task<APIResponse<bool>> ConfirmMailAsync(ConfirmMailViewModel request);
        Task<APIResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordViewModel request);
        Task<APIResponse<bool>> CompleteProfileAsync(string userId, CompleteProfileViewModel request);
        Task<APIResponse<bool>> ValidateUserAsync(string userId, string email);
    }
}
