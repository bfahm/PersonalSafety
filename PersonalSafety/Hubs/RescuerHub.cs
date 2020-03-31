﻿using Microsoft.AspNetCore.Authorization;
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
        private readonly IAgentHub _agentHub;

        public RescuerHub(IHubContext<RescuerHub> hubContext, IPersonnelRepository personnelRepository, IAgentHub agentHub)
        {
            _hubContext = hubContext;
            _personnelRepository = personnelRepository;
            _agentHub = agentHub;
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

        public void MakeRescuerIdle(string rescuerEmail)
        {
            RescuerConnectionInfo rescuerConnection = TrackerHandler.RescuerConnectionInfoSet.FirstOrDefault(r => r.UserEmail == rescuerEmail);
            if (rescuerConnection != null)
            {
                rescuerConnection.CurrentJob = 0;
            }

            RescuerConnectionInfo rescuerOfflineState = TrackerHandler.RescuerWithPendingMissionsSet.FirstOrDefault(r => r.UserEmail == rescuerEmail);
            if (rescuerOfflineState != null)
            {
                rescuerOfflineState.CurrentJob = 0;
            }
        }

        public override async Task OnConnectedAsync()
        {
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
                TrackerHandler.RescuerWithPendingMissionsSet.Remove(recurrentConnection);
                NotifyNewChanges(recurrentConnection.CurrentJob, recurrentConnection.UserEmail);
                HubTools.PrintToConsole(recurrentConnection.UserEmail, "had a mission with id: "+ recurrentConnection.CurrentJob + " state saved and now restored to him");
            }

            // Notify Agent in the same hub that rescuers state has changed.
            await _agentHub.NotifyChangeInRescuers(currentConnection.DepartmentId);

            // Call the base class in the final step, to allow it for connection info retrieval.
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
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

                // Notify Agent in the same hub that rescuers state has changed.
                await _agentHub.NotifyChangeInRescuers(currentDisconnection.DepartmentId);
            }
        }
    }
}
