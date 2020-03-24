using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PersonalSafety.Hubs.HubTracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace PersonalSafety.Hubs
{
    [Authorize]
    public class MainHub : Hub, IMainHub
    {
        public static readonly string connectionInfoChannel = "ConnectionInfoChannel";
        public Task GetMyConnectionInfo()
        {
            var json = JsonSerializer.Serialize(UserHandler.ConnectionInfoSet.Where(c=>c.ConnectionId == Context.ConnectionId).FirstOrDefault());
            return Clients.Caller.SendAsync(connectionInfoChannel, json);
        }

        public bool isConnected(string connectionId)
        {
            return UserHandler.ConnectionInfoSet.Where(c => c.ConnectionId == connectionId).FirstOrDefault() != null;
        }

        public override async Task OnConnectedAsync()
        {
            ConnectionInfo currentConnection = new ConnectionInfo
            {
                ConnectionId = Context.ConnectionId,
                UserId = Context.User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value,
                UserEmail = Context.User.FindFirst(ClaimTypes.Email).Value
            };
            
            UserHandler.ConnectionInfoSet.Add(currentConnection);

            string consoleLine = currentConnection.UserEmail + " has connected to the server with connection id: " + currentConnection.ConnectionId;
            ConsoleHandler.ConsoleSet.Add(consoleLine);
            Console.WriteLine(consoleLine);
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            // Remove user from user trackers
            ConnectionInfo currentDisconnection = UserHandler.ConnectionInfoSet.Where(c => c.ConnectionId == Context.ConnectionId).FirstOrDefault();
            if(currentDisconnection != null)
            {
                UserHandler.ConnectionInfoSet.Remove(currentDisconnection);

                string consoleLine = currentDisconnection.UserEmail + " has disconnected from the server, he had connection id: " + currentDisconnection.ConnectionId;
                ConsoleHandler.ConsoleSet.Add(consoleLine);
                Console.WriteLine(consoleLine);
            }

            // Remove user requests from SOS trackers
            int currentOngoingSOSes = SOSHandler.SOSInfoSet.Where(c => c.ConnectionId == Context.ConnectionId).Count();
            if (currentOngoingSOSes>0)
            {
                SOSHandler.SOSInfoSet.RemoveWhere(sc => sc.ConnectionId == Context.ConnectionId);
                
                string consoleLine = currentDisconnection.UserEmail + " had some SOS requests that where tracked and now dismissed.";
                ConsoleHandler.ConsoleSet.Add(consoleLine);
                Console.WriteLine(consoleLine);
            }

            await base.OnDisconnectedAsync(ex);
        }
    }
}
