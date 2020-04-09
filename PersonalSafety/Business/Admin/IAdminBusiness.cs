using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels.AdminVM;

namespace PersonalSafety.Business
{
    public interface IAdminBusiness
    {
        APIResponse<List<GetDepartmentDataViewModel>> GetDepartments();
        Task<APIResponse<bool>> RegisterAgentAsync(RegisterAgentViewModel request);
    }
}
