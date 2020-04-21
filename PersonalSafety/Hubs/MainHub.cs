using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PersonalSafety.Hubs.HubTracker;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using PersonalSafety.Hubs.Helpers;

namespace PersonalSafety.Hubs
{
    [Authorize]
    public class MainHub : Hub, IMainHub
    {
        private static readonly string ConnectionInfoChannel = "ConnectionInfoChannel";
        
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

            // Print to the console
            HubTools.PrintToConsole(currentConnection.UserEmail, currentConnection.ConnectionId, false);

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
                HubTools.PrintToConsole(currentDisconnection.UserEmail, currentDisconnection.ConnectionId, true);
            }

            await base.OnDisconnectedAsync(ex);
        }
    }
}
