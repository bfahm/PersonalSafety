using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PersonalSafety.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Hubs.Helpers;
using PersonalSafety.Hubs.HubTracker;
using PersonalSafety.Hubs.Services;
using PersonalSafety.Models;

namespace PersonalSafety.Hubs
{
    [Authorize(Roles =Roles.ROLE_RESCUER)]
    public class RescuerHub : MainHub, IRescuerHub
    {
        private readonly string channelName = "RescuerChannel";
        private readonly IHubContext<RescuerHub> _hubContext;
        private readonly IPersonnelRepository _personnelRepository;

        public RescuerHub(IHubContext<RescuerHub> hubContext, IPersonnelRepository personnelRepository)
        {
            _hubContext = hubContext;
            _personnelRepository = personnelRepository;
        }

        public bool NotifyNewChanges(int requestId, string rescuerEmail)
        {
            RescuerConnectionInfo connectionInformation = TrackerHandler.RescuerConnectionInfoSet.FirstOrDefault(r => r.UserEmail == rescuerEmail);

            if (connectionInformation != null)
            {
                // Sending to rescuer the id of the request to fetch its data using his API endpoint.
                _hubContext.Clients.Client(connectionInformation.ConnectionId).SendAsync(channelName, requestId);

                connectionInformation.CurrentJob = requestId;

                return true;
            }

            return false;
        }

        public override async Task OnConnectedAsync()
        {
            // On connection, do what the base function was going to do, then continue with custom implementation
            await base.OnConnectedAsync();

            // Send to the Rescuer his connectionID whenever he connects through the hub
            // Note that GetMyConnectionInfo also works, and also sends its information on the same channel, 
            // but when invoked, the whole connectionInfo object is sent through a JSON string.
            await Clients.Caller.SendAsync(connectionInfoChannel, TrackerHandler.ConnectionInfoSet.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId)?.ConnectionId);

            // Track the current connection in a different Rescuer Hash-list
            RescuerConnectionInfo currentConnection = new RescuerConnectionInfo
            {
                ConnectionId = Context.ConnectionId,
                UserEmail = Context.User.FindFirst(ClaimTypes.Email).Value,
                DepartmentId = _personnelRepository.GetPersonnelDepartment(Context.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value).Id
            };
            TrackerHandler.RescuerConnectionInfoSet.Add(currentConnection);

            // Find out if the rescuer had disconnected before and is trying to reconnect now
            RescuerConnectionInfo recurrentConnection = TrackerHandler.RescuerWithPendingMissionsSet.FirstOrDefault(c => c.UserEmail == Context.User.FindFirst(ClaimTypes.Email).Value);
            if (recurrentConnection != null && recurrentConnection.CurrentJob > 0)
            {
                //If so, send him his previous state and remove that state from the tracker.
                TrackerHandler.RescuerConnectionInfoSet.Remove(recurrentConnection);
                NotifyNewChanges(recurrentConnection.CurrentJob, recurrentConnection.UserEmail);
                HubTools.PrintToConsole(recurrentConnection.UserEmail, "had a mission with id: "+ recurrentConnection.CurrentJob + " state saved and now restored to him");
            }
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            // On disconnection, do what the base function was going to do, then continue with custom implementation
            await base.OnDisconnectedAsync(ex);

            // Remove user from user trackers
            RescuerConnectionInfo currentDisconnection = TrackerHandler.RescuerConnectionInfoSet.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            if (currentDisconnection != null)
            {
                TrackerHandler.RescuerConnectionInfoSet.Remove(currentDisconnection);

                // Check if user who disconnected had an ongoing mission
                if (currentDisconnection.CurrentJob > 0)
                {
                    TrackerHandler.RescuerWithPendingMissionsSet.Add(currentDisconnection);

                    // Write a summary to the console.
                    HubTools.PrintToConsole(currentDisconnection.UserEmail, "was helping a client with request id: " + currentDisconnection.CurrentJob + ", his state was saved until he's back on.");
                }
            }
        }
    }
}
