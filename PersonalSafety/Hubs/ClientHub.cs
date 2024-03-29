﻿using Microsoft.AspNetCore.SignalR;
using PersonalSafety.Hubs.HubTracker;
using PersonalSafety.Contracts.Enums;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PersonalSafety.Hubs
{
    public class ClientHub : MainHub, IClientHub
    {
        private readonly string channelName = "ClientChannel";
        private readonly string eventsChannelName = "ClientEventsChannel";
        private readonly IHubContext<ClientHub> _hubContext;

        public ClientHub(IHubContext<ClientHub> hubContext, ILogger<ClientHub> logger) : base(logger)
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

        public void RemoveClientFromTrackers(string userId)
        {
            TrackerHandler.ClientConnectionInfoSet.RemoveWhere(sc => sc.UserId == userId);
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

        public async Task JoinEventRoom(string userEmail, int eventId)
        {
            var roomName = GenerateRoomName(eventId);
            var connectionInfo = TrackerHandler.ClientConnectionInfoSet.FirstOrDefault(c => c.UserEmail == userEmail);

            if(connectionInfo != null)
            {
                await _hubContext.Groups.AddToGroupAsync(connectionInfo.ConnectionId, roomName);
                await _hubContext.Clients.Group(roomName).SendAsync(eventsChannelName, $"CLIENT {connectionInfo.UserEmail} JOINED {roomName}.");
            }
        }

        public async Task LeaveEventRoom(string userEmail, int eventId)
        {
            var roomName = GenerateRoomName(eventId);
            var connectionInfo = TrackerHandler.ClientConnectionInfoSet.FirstOrDefault(c => c.UserEmail == userEmail);

            if (connectionInfo != null)
            {
                await _hubContext.Groups.RemoveFromGroupAsync(connectionInfo.ConnectionId, roomName);
                await _hubContext.Clients.Group(roomName).SendAsync(eventsChannelName, $"CLIENT {connectionInfo.UserEmail} LEFT {roomName}.");
            }
        }

        public async Task SendToEventRoom(string userEmail, int eventId, double latitude, double longitude)
        {
            var roomName = GenerateRoomName(eventId);
            await _hubContext.Clients.Group(roomName).SendAsync(eventsChannelName, userEmail, latitude, longitude);
        }

        private string GenerateRoomName(int eventId)
        {
            return $"EVENT_ROOM_{eventId}";
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
