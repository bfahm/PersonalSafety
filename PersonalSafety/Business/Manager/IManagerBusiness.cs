using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels;
using PersonalSafety.Models.ViewModels.AdminVM;
using PersonalSafety.Models.ViewModels.ManagerVM;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalSafety.Business
{
    public interface IManagerBusiness
    {
        // Stats Endpoint
        Task<APIResponse<TopCardsDataViewModel>> GetTopCardsDataAsync(string userId);
        Task<APIResponse<SOSPieDataViewModel>> GetSOSPieDataAsync(string userId);
        Task<APIResponse<List<SOSChartDataViewModel>>> GetSOSChartDataAsync(string userId);

        // Departments Endpoint
        Task<APIResponse<List<GetDepartmentDataViewModel>>> GetDepartmentsAsync(string userId);
        Task<APIResponse<List<GetSOSRequestViewModel>>> GetDepartmentRequestsAsync(string userId, int departmentId, int? requestState, bool enforceClaims);
    }
}
