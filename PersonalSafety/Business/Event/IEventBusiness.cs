using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels.ClientVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Business
{
    public interface IEventsBusiness
    {
        Task<APIResponse<PostEventResponseViewModel>> PostEventAsync(string userId, PostEventRequestViewModel request);
    }
}
