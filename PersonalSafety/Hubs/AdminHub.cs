using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Hubs.HubTracker;

namespace PersonalSafety.Hubs
{
    [Authorize(Roles = Roles.ROLE_ADMIN)]
    public class AdminHub : MainHub, IAdminHub
    {
        private readonly IHubContext<AdminHub> _hubContext;
        
        public AdminHub(IHubContext<AdminHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task PrintToOnlineConsole(string text)
        {
            return _hubContext.Clients.All.SendAsync("AdminConsoleChanges", text);
        }
    }
}
