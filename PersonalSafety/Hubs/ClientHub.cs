using Microsoft.AspNetCore.SignalR;
using PersonalSafety.Hubs.HubTracker;
using PersonalSafety.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using PersonalSafety.Hubs.Helpers;

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

        public bool TrackSOSIdForClient(string clientEmail, int sosRequestId)
        {
            ClientConnectionInfo currentRequest =
                TrackerHandler.ClientConnectionInfoSet.FirstOrDefault(c => c.UserEmail == clientEmail);

            if (currentRequest == null || string.IsNullOrEmpty(currentRequest.ConnectionId))
            {
                return false;
            }

            currentRequest.SOSId = sosRequestId;
            return true;
        }

        public void RemoveClientFromTrackers(int sosRequestId)
        {
            TrackerHandler.ClientConnectionInfoSet.RemoveWhere(sc => sc.SOSId == sosRequestId);
        }

        /// Returns True if user had a maintained connection with the server, else false
        public bool NotifyUserSOSState(int sosRequestId, int sosRequestState)
        {
            ClientConnectionInfo connectionInformation = TrackerHandler.ClientConnectionInfoSet.FirstOrDefault(r => r.SOSId == sosRequestId);

            if (connectionInformation != null && !string.IsNullOrEmpty(connectionInformation.ConnectionId))
            {
                _hubContext.Clients.Client(connectionInformation.ConnectionId).SendAsync(channelName, sosRequestId, ((StatesTypesEnum)sosRequestState).ToString());

                if (sosRequestState == (int) StatesTypesEnum.Canceled)
                {
                    connectionInformation.SOSId = 0;
                }

                return true;
            }
            return false;
        }


        public override async Task OnConnectedAsync()
        {
            var currentReconnection = TrackerHandler.ClientConnectionInfoSet.FirstOrDefault(sc => sc.UserEmail == Context.User.FindFirst(ClaimTypes.Email).Value);

            if (currentReconnection != null)
            {
                // Renew user connectionId if he is trying to reconnect.
                currentReconnection.ConnectionId = Context.ConnectionId;
            }
            else
            {
                ClientConnectionInfo currentConnection = new ClientConnectionInfo
                {
                    ConnectionId = Context.ConnectionId,
                    UserId = Context.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value,
                    UserEmail = Context.User.FindFirst(ClaimTypes.Email).Value
                };

                TrackerHandler.ClientConnectionInfoSet.Add(currentConnection);
            }

            // Call the base class in the final step, to allow it for connection info retrieval.
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            var currentDisconnection = TrackerHandler.ClientConnectionInfoSet.FirstOrDefault(sc => sc.ConnectionId == Context.ConnectionId);

            if (currentDisconnection != null)
            {
                // Nullify user connectionId for reconnection support.
                currentDisconnection.ConnectionId = "";
            }

            await base.OnDisconnectedAsync(ex);
        }
    }
}
