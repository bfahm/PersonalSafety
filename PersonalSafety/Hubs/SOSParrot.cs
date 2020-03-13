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
    public class SOSParrot : MainHub
    {
        public Task GetMyConnectionId()
        {
            return Clients.Caller.SendAsync("ReceiveMessage", Context.ConnectionId);
        }
    }
}
