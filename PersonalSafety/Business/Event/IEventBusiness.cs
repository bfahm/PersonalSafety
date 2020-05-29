using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels.ClientVM;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalSafety.Business
{
    public interface IEventsBusiness
    {
        APIResponse<string> UpdateLastKnownLocation(string userId, LocationViewModel request);
        APIResponse<bool> UpdateDeviceRegistraionKey(string userId, DeviceRegistrationViewModel request);

        Task<APIResponse<PostEventResponseViewModel>> PostEventAsync(string userId, PostEventRequestViewModel request);
        Task<APIResponse<List<EventMinifiedViewModel>>> GetEventsAsync(string userId, int? filter);
        Task<APIResponse<EventDetailedViewModel>> GetEventByIdAsync(int eventId);
    }
}
