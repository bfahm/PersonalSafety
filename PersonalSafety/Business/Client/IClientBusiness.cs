using PersonalSafety.Models.ViewModels;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels.AccountVM;

namespace PersonalSafety.Business
{
    public interface IClientBusiness
    {
        Task<APIResponse<bool>> RegisterAsync(RegistrationViewModel request);
        Task<APIResponse<ProfileViewModel>> GetProfileAsync(string userId);
        APIResponse<bool> EditProfile(string userId, ProfileViewModel request);
        Task<APIResponse<LoginResponseViewModel>> LoginWithFacebookAsync(string accessToken);
        Task<APIResponse<bool>> RegisterWithFacebookAsync(RegistrationWithFacebookViewModel request);
    }
}
