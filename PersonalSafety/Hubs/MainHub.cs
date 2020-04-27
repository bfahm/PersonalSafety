using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PersonalSafety.Hubs.HubTracker;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using PersonalSafety.Hubs.Helpers;
using Microsoft.Extensions.Logging;

namespace PersonalSafety.Hubs
{
    [Authorize]
    public class MainHub : Hub, IMainHub
    {
        private static readonly string ConnectionInfoChannel = "ConnectionInfoChannel";
        private readonly ILogger<MainHub> _logger;

        public MainHub(){ }
        public MainHub(ILogger<MainHub> logger)
        {
            _logger = logger;
        }

        public bool isConnected(string userId)
        {
            return TrackerHandler.AllConnectionInfoSet.FirstOrDefault(c => c.UserId == userId) != null;
        }

        
        public override async Task OnConnectedAsync()
        {
            // Save connection info in the tracker
            ClientConnectionInfo currentConnection = new ClientConnectionInfo
            {
                ConnectionId = Context.ConnectionId,
                UserId = Context.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value,
                UserEmail = Context.User.FindFirst(ClaimTypes.Email).Value
            };

            TrackerHandler.AllConnectionInfoSet.Add(currentConnection);

            // Log to the consoles
            _logger?.LogInformation(HubConsoleHelper.ConsoleFormater(currentConnection.UserEmail, currentConnection.ConnectionId, false));

            // Automatically send client data to him after connection.
            await Clients.Caller.SendAsync(ConnectionInfoChannel, currentConnection.ConnectionId, currentConnection.UserEmail);
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            var currentDisconnection = TrackerHandler.AllConnectionInfoSet.FirstOrDefault(sc => sc.ConnectionId == Context.ConnectionId);

            if (currentDisconnection != null)
            {
                TrackerHandler.AllConnectionInfoSet.RemoveWhere(c=> c.UserEmail == currentDisconnection.UserEmail);
                _logger.LogInformation(HubConsoleHelper.ConsoleFormater(currentDisconnection.UserEmail, currentDisconnection.ConnectionId, true));
            }

            await base.OnDisconnectedAsync(ex);
        }
    }
}
