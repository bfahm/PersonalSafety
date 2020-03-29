using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Contracts;

namespace PersonalSafety.Business
{
    public interface IAdminBusiness
    {
        Task<APIResponse<bool>> RegisterAgentAsync(RegisterAgentViewModel request);
    }
}
