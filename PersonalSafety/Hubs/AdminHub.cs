using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using PersonalSafety.Contracts;

namespace PersonalSafety.Hubs
{
    [Authorize(Roles = Roles.ROLE_ADMIN)]
    public class AdminHub : MainHub, IAdminHub
    {
        private readonly IHubContext<AdminHub> _hubContext;
        public static readonly string ChannelNotifyTrackerChanges = "AdminTrackerChanges";
        public static readonly string ChannelNotifyConsoleChanges = "AdminConsoleChanges";

        public AdminHub(IHubContext<AdminHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task NotifyChanges(string channelName)
        {
            return _hubContext.Clients.All.SendAsync(channelName);
        }
    }
}
