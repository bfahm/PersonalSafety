using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels;
using PersonalSafety.Models.ViewModels.AdminVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Business
{
    public interface IManagerBusiness
    {
        Task<APIResponse<List<GetDepartmentDataViewModel>>> GetDepartmentsAsync(string userId);
        Task<APIResponse<List<GetSOSRequestViewModel>>> GetDepartmentRequestsAsync(string userId, int departmentId, int? requestState, bool enforceClaims);
    }
}
