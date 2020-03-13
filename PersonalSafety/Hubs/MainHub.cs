using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PersonalSafety.Hubs.HubHelper;
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
        public Task GetConnectionInfo()
        {
            var json = JsonSerializer.Serialize(UserHandler.ConnectionInfoSet.ToList());
            return Clients.Caller.SendAsync("ReceiveMessage", json);
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

            Console.WriteLine(currentConnection.UserEmail + " has connected to the server with connection id: " + currentConnection.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            ConnectionInfo currentDisconnection = UserHandler.ConnectionInfoSet.Where(c => c.ConnectionId == Context.ConnectionId).FirstOrDefault();
            
            if(currentDisconnection != null)
            {
                UserHandler.ConnectionInfoSet.Remove(currentDisconnection);
                Console.WriteLine(currentDisconnection.UserEmail + " has disconnected from the server, he had connection id: " + currentDisconnection.ConnectionId);
            }

            await base.OnDisconnectedAsync(ex);
        }
    }
}
