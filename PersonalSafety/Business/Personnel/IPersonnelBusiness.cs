using PersonalSafety.Helpers;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Business
{
    public interface IPersonnelBusiness
    {
        Task<APIResponse<List<GetSOSRequestViewModel>>> GetRelatedRequestsAsync(string userId, int? requestState);
    }
}
