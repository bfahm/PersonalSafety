using Microsoft.AspNetCore.SignalR;
using PersonalSafety.Hubs.HubTracker;
using PersonalSafety.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PersonalSafety.Hubs
{
    public class ClientHub : MainHub, IClientHub
    {
        private readonly string channelName = "ClientChannel";
        private readonly IHubContext<ClientHub> _hubContext;

        public ClientHub(IHubContext<ClientHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// Returns True if user had a maintained connection with the server, else false
        public bool NotifyUserSOSState(int sosRequestId, int sosRequestState)
        {
            SOSInfo connectionInformation = SOSHandler.SOSInfoSet.Where(r => r.SOSId == sosRequestId).FirstOrDefault();

            if (connectionInformation != null)
            {
                var jsonMsg = JsonSerializer.Serialize(new { requestId = sosRequestId, requestState = ((StatesTypesEnum)sosRequestState).ToString() });
                _hubContext.Clients.Client(connectionInformation.ConnectionId).SendAsync(channelName, jsonMsg);

                return true;
            }
            return false;
        }
    }
}
