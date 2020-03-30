using Microsoft.AspNetCore.SignalR;
using PersonalSafety.Hubs.HubTracker;
using PersonalSafety.Contracts.Enums;
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
            SOSInfo connectionInformation = TrackerHandler.SOSInfoSet.FirstOrDefault(r => r.SOSId == sosRequestId);

            if (connectionInformation != null)
            {
                _hubContext.Clients.Client(connectionInformation.ConnectionId).SendAsync(channelName, sosRequestId, ((StatesTypesEnum)sosRequestState).ToString());

                return true;
            }
            return false;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            
            // Send to the client his connectionID whenever he connects through the clienthub
            // Note that GetMyConnectionInfo also works, and also sends its information on the same channel, 
            // but when invoked, the whole connectionInfo object is sent through a JSON string.
            await Clients.Caller.SendAsync(connectionInfoChannel, TrackerHandler.ConnectionInfoSet.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId)?.ConnectionId);
        }
    }
}
