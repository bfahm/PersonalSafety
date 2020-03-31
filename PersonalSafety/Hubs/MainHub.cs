using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PersonalSafety.Hubs.HubTracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using PersonalSafety.Hubs.Helpers;

namespace PersonalSafety.Hubs
{
    [Authorize]
    public class MainHub : Hub, IMainHub
    {
        public static readonly string connectionInfoChannel = "ConnectionInfoChannel";
        
        public bool isConnected(string connectionId)
        {
            return TrackerHandler.AllConnectionInfoSet.FirstOrDefault(c => c.ConnectionId == connectionId) != null;
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
            await Clients.Caller.SendAsync(connectionInfoChannel, currentConnection.ConnectionId, currentConnection.UserEmail);
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            var currentDisconnection = TrackerHandler.ClientConnectionInfoSet.FirstOrDefault(sc => sc.ConnectionId == Context.ConnectionId);

            if (currentDisconnection != null)
            {
                HubTools.PrintToConsole(currentDisconnection.UserEmail, currentDisconnection.ConnectionId, true);
            }

            await base.OnDisconnectedAsync(ex);
        }
    }
}
