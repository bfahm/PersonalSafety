using PersonalSafety.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels.AdminVM;
using PersonalSafety.Models;

namespace PersonalSafety.Business
{
    public interface IAdminBusiness
    {
        APIResponse<List<GetDepartmentDataViewModel>> GetDepartments();
        Task<APIResponse<bool>> RegisterAgentAsync(RegisterAgentViewModel request);
        APIResponse<Dictionary<string, object>> RetrieveTrackers();
        APIResponse<object> RetrieveConsole();
        APIResponse<bool> ResetTrackers();
        APIResponse<bool> ResetConsole();
        APIResponse<bool> ResetRescuerState(string rescuerEmail);
        APIResponse<bool> ResetClientState(string clientEmail);
        APIResponse<DistributionTreeViewModel> GetDistributionTree();
        APIResponse<DistributionTreeViewModel> AddNewDistribution(NewDistributionRequestViewModel request);
        APIResponse<DistributionTreeViewModel> RenameDistribution(RenameDistributionRequestViewModel request);
    }
}
