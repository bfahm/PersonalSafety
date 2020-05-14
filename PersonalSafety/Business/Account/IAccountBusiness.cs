using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels.AccountVM;

namespace PersonalSafety.Business
{
    public interface IAccountBusiness
    {
        Task<APIResponse<LoginResponseViewModel>> LoginAsync(LoginRequestViewModel request);
        Task<APIResponse<AuthenticationDetailsViewModel>> RefreshTokenAsync(RefreshTokenRequestViewModel request);
        Task<APIResponse<bool>> ResetPasswordAsync(string email);
        Task<APIResponse<bool>> SubmitResetPasswordAsync(ResetPasswordViewModel request);
        Task<APIResponse<bool>> SendConfirmMailAsync(string email);
        Task<APIResponse<bool>> SubmitConfirmationAsync(ConfirmMailViewModel request);
        Task<APIResponse<bool>> ChangeEmailAsync(string userId, string newEmail);
        Task<APIResponse<bool>> SubmitChangeEmailAsync(ChangeEmailViewModel request);
        Task<APIResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordViewModel request);
        Task<APIResponse<bool>> ValidateTokenAsync(string token);
        APIResponse<AccountBasicInfoViewModel> GetBasicInfo(string currentlyLoggedInUserId);
    }
}
