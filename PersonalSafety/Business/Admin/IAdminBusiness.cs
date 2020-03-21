using PersonalSafety.Options;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Helpers;

namespace PersonalSafety.Business
{
    public interface IAdminBusiness
    {
        Task<APIResponse<bool>> RegisterPersonnelAsync(RegisterPersonnelViewModel request);
    }
}
