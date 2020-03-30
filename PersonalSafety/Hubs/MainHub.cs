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
        public Task GetMyConnectionInfo()
        {
            var json = JsonSerializer.Serialize(TrackerHandler.ConnectionInfoSet.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId));
            return Clients.Caller.SendAsync(connectionInfoChannel, json);
        }

        public bool isConnected(string connectionId)
        {
            return TrackerHandler.ConnectionInfoSet.FirstOrDefault(c => c.ConnectionId == connectionId) != null;
        }

        public override async Task OnConnectedAsync()
        {
            ConnectionInfo currentConnection = new ConnectionInfo
            {
                ConnectionId = Context.ConnectionId,
                UserId = Context.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value,
                UserEmail = Context.User.FindFirst(ClaimTypes.Email).Value
            };

            TrackerHandler.ConnectionInfoSet.Add(currentConnection);

            HubTools.PrintToConsole(currentConnection.UserEmail, currentConnection.ConnectionId, false);
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            // Remove user from user trackers
            ConnectionInfo currentDisconnection = TrackerHandler.ConnectionInfoSet.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            if(currentDisconnection != null)
            {
                TrackerHandler.ConnectionInfoSet.Remove(currentDisconnection);

                string consoleLine = currentDisconnection.UserEmail + " has disconnected from the server, he had connection id: " + currentDisconnection.ConnectionId;
                TrackerHandler.ConsoleSet.Add(consoleLine);
                Console.WriteLine(consoleLine);
            }

            // Remove user requests from SOS trackers
            int currentOngoingSOSes = TrackerHandler.SOSInfoSet.Count(c => c.ConnectionId == Context.ConnectionId);
            if (currentOngoingSOSes>0)
            {
                TrackerHandler.SOSInfoSet.RemoveWhere(sc => sc.ConnectionId == Context.ConnectionId);
                
                HubTools.PrintToConsole(currentDisconnection.UserEmail, currentDisconnection.ConnectionId, true);
            }

            await base.OnDisconnectedAsync(ex);
        }
    }
}
