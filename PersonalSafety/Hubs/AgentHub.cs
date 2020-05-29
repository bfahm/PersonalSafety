using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PersonalSafety.Contracts.Enums;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Hubs.HubTracker;
using PersonalSafety.Models;
using Microsoft.Extensions.Logging;

namespace PersonalSafety.Hubs
{
    [Authorize(Roles =Roles.ROLE_AGENT)]
    public class AgentHub : MainHub, IAgentHub
    {
        // Channel naming convention: "[NameOfChannelAccessor][NameOfSubjectOfChannel]Channel"
        private const string RequestsChannelName = "AgentRequestsChannel";
        private const string RescuersChannelName = "AgentRescuersChannel";

        private readonly IHubContext<AgentHub> _hubContext;
        private readonly IPersonnelRepository _personnelRepository;

        public AgentHub(IHubContext<AgentHub> hubContext, IPersonnelRepository personnelRepository, ILogger<AgentHub> logger) : base(logger)
        {
            _personnelRepository = personnelRepository;
            _hubContext = hubContext;
        }

        public void NotifyNewChanges(int requestId, int requestState, string departmentName)
        {
            var jsonMsg = JsonSerializer.Serialize(new { requestId = requestId, requestState = ((StatesTypesEnum)requestState).ToString() });

            var onlineAgent = TrackerHandler.AgentConnectionInfoSet.FirstOrDefault(a => a.DepartmentName == departmentName);

            if (onlineAgent != null)
                _hubContext.Clients.Client(onlineAgent.ConnectionId).SendAsync(RequestsChannelName, jsonMsg);
        }

        public void NotifyChangeInRescuers(string departmentName)
        {
            var onlineAgent = TrackerHandler.AgentConnectionInfoSet.FirstOrDefault(a => a.DepartmentName == departmentName);

            if (onlineAgent != null)
                _hubContext.Clients.Client(onlineAgent.ConnectionId).SendAsync(RescuersChannelName);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            AgentConnectionInfo currentConnection = new AgentConnectionInfo()
            {
                ConnectionId = Context.ConnectionId,
                UserId = userId,
                UserEmail = Context.User.FindFirst(ClaimTypes.Email).Value,
                DepartmentName = _personnelRepository.GetPersonnelDepartment(userId).ToString()
            };

            TrackerHandler.AgentConnectionInfoSet.Add(currentConnection);

            // Call the base class in the final step, to allow it for connection info retrieval.
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            var currentDisconnection = TrackerHandler.AgentConnectionInfoSet.FirstOrDefault(sc => sc.ConnectionId == Context.ConnectionId);

            if (currentDisconnection != null)
            {
                TrackerHandler.AgentConnectionInfoSet.RemoveWhere(c => c.UserId == currentDisconnection.UserId);
            }

            await base.OnDisconnectedAsync(ex);
        }
    }
}
