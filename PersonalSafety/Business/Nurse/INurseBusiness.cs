using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels;
using PersonalSafety.Models.ViewModels.NurseVM;
using System.Threading.Tasks;

namespace PersonalSafety.Business.Nurse
{
    public interface INurseBusiness
    {
        Task<APIResponse<GetUserDataViewModel>> GetClientDetails(string clientId);
        Task<APIResponse<VictimStateViewModel>> EditClientVictimState(string clientEmail, bool isVictim);
    }
}
