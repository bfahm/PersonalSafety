using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels.ClientVM;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalSafety.Business
{
    public interface IEventsBusiness
    {
        // Client Actions
        APIResponse<string> UpdateLastKnownLocation(string userId, LocationViewModel request);
        APIResponse<bool> UpdateDeviceRegistraionKey(string userId, DeviceRegistrationViewModel request);

        Task<APIResponse<PostEventResponseViewModel>> PostEventAsync(string userId, PostEventRequestViewModel request);
        Task<APIResponse<List<EventDetailedViewModel>>> GetEventsDetailedAsync(string userId, int? filter);
        Task<APIResponse<EventDetailedViewModel>> GetEventByIdAsync(int eventId);
        
        Task<APIResponse<bool>> CancelEventByIdAsync(string userId, int eventId);
        Task<APIResponse<bool>> SolveEventByIdAsync(string userId, int eventId);

        // Managerial Actions
        Task<APIResponse<List<EventDetailedViewModel>>> GetEventsForManagerAsync(string managerUserId);
        Task<APIResponse<bool>> UpdateEventValidity(string managerUserId, int eventId, bool toValidate);
    }
}
