using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PersonalSafety.Hubs.HubTracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PersonalSafety.Contracts;

namespace PersonalSafety.Hubs
{
    [Authorize(Roles = Roles.ROLE_ADMIN)]
    public class AdminHub : MainHub
    {
        public Task GetConnectionInfo()
        {
            var json = JsonSerializer.Serialize(UserHandler.ConnectionInfoSet.ToList());
            return Clients.Caller.SendAsync("AdminGetConnectionInfo", json);
        }

        public Task GetConsoleLines()
        {
            var json = JsonSerializer.Serialize(ConsoleHandler.ConsoleSet.ToList());
            return Clients.Caller.SendAsync("AdminGetConsoleLines", json);
        }
        
        public void ClearConsoleLines()
        {
            ConsoleHandler.ConsoleSet.Clear();
        }
    }
}
