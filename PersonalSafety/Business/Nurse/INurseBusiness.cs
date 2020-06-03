using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels;
using System.Threading.Tasks;

namespace PersonalSafety.Business.Nurse
{
    public interface INurseBusiness
    {
        Task<APIResponse<GetUserDataViewModel>> GetClientDetails(string clientId);
    }
}
