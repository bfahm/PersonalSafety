using PersonalSafety.Options;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Helpers;
using PersonalSafety.Contracts;

namespace PersonalSafety.Business
{
    public interface IPersonnelBusiness
    {
        Task<APIResponse<List<GetSOSRequestViewModel>>> GetRelatedRequestsAsync(string userId, int? requestState);
    }
}
