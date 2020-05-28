using PersonalSafety.Contracts;
using PersonalSafety.Contracts.Enums;
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
        Task<APIResponse<List<EventMinifiedViewModel>>> GetEventsAsync(string userId, int? filter);
    }
}
