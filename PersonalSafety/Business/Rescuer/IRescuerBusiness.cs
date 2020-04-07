using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels;

namespace PersonalSafety.Business
{
    public interface IRescuerBusiness
    {
        Task<APIResponse<GetSOSRequestViewModel>> GetSOSRequestDetailsAsync(string userId, int requestId);
    }
}
