using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PersonalSafety.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PersonalSafety.Contracts;

namespace PersonalSafety.Hubs
{
    [Authorize(Roles =Roles.ROLE_AGENT)]
    public class AgentHub : MainHub, IAgentHub
    {
        // Channel naming convention: "[NameOfChannelAccessor][NameOfSubjectOfChannel]Channel"
        private const string RequestsChannelName = "AgentRequestsChannel";
        private const string RescuersChannelName = "AgentRescuersChannel";

        private readonly IHubContext<AgentHub> _hubContext;

        public AgentHub(IHubContext<AgentHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task NotifyNewChanges(int requestId, int requestState)
        {
            var jsonMsg = JsonSerializer.Serialize(new { requestId = requestId, requestState = ((StatesTypesEnum)requestState).ToString() });
            
            return _hubContext.Clients.All.SendAsync(RequestsChannelName, jsonMsg);
        }

        public Task NotifyChangeInRescuers(int departmentId)
        {
            return _hubContext.Clients.All.SendAsync(RescuersChannelName);
        }
    }
}
