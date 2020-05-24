using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PersonalSafety.Contracts;
using PersonalSafety.Hubs.HubTracker;
using PersonalSafety.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Hubs
{
    [Authorize(Roles = Roles.ROLE_ADMIN + "," + Roles.ROLE_AGENT + "," + Roles.ROLE_RESCUER)]
    public class LocationHub : Hub
    {
        private readonly IPersonnelRepository _personnelRepository;
        private readonly string locationChannelName = "LocationChannel";
        private readonly string infoChannelName = "InfoChannel";
        private readonly string alertsChannelName = "AlertsChannel";

        public LocationHub(IPersonnelRepository personnelRepository)
        {
            _personnelRepository = personnelRepository;
        }

        public Task ShareLocation(string departmentName, string location)
        {
            return Clients.Group(departmentName).SendAsync(locationChannelName, location);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            var isAgent = Context.User.Claims.FirstOrDefault(x => x.Value == Roles.ROLE_AGENT) != null;
            var isRescuer = Context.User.Claims.FirstOrDefault(x => x.Value == Roles.ROLE_RESCUER) != null;

            if (isAgent || isRescuer)
            {
                var department = _personnelRepository.GetPersonnelDepartment(userId);

                if (isAgent)
                {
                    var departmentChatGroup = await GetOrAddGroup(department);
                    departmentChatGroup.ActiveAgents += 1;

                    await Groups.AddToGroupAsync(Context.ConnectionId, departmentChatGroup.DepartmentName);
                    await Clients.Group(departmentChatGroup.DepartmentName).SendAsync(alertsChannelName, $"{userId} / AGENT JOINED!");

                    await Clients.Caller.SendAsync(infoChannelName, department.ToString());
                }
                else if (isRescuer)
                {
                    var departmentChatGroup = GetGroup(department);

                    if(departmentChatGroup != null)
                    {
                        departmentChatGroup.ActiveRescuers += 1;

                        await Groups.AddToGroupAsync(Context.ConnectionId, departmentChatGroup.DepartmentName);
                        await Clients.Group(departmentChatGroup.DepartmentName).SendAsync(alertsChannelName, $"{userId} / RESUCER  JOINED!");

                        await Clients.Caller.SendAsync(infoChannelName, department.ToString());
                    }
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            var userId = Context.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            var isAgent = Context.User.Claims.FirstOrDefault(x => x.Value == Roles.ROLE_AGENT) != null;
            var isRescuer = Context.User.Claims.FirstOrDefault(x => x.Value == Roles.ROLE_RESCUER) != null;

            if (isAgent || isRescuer)
            {
                var department = _personnelRepository.GetPersonnelDepartment(userId);
                var currentGroup = GetGroup(department);

                if (currentGroup != null)
                {
                    if (isAgent)
                    {
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, currentGroup.DepartmentName);
                        currentGroup.ActiveAgents -= 1;

                        await RemoveGroupIfIdle(currentGroup);
                    }
                    else if (isRescuer)
                    {
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, currentGroup.DepartmentName);
                        currentGroup.ActiveRescuers -= 1;

                        await Clients.Group(currentGroup.DepartmentName).SendAsync(alertsChannelName, $"{userId} / RESUCER  DISCONNECTED!");
                    }
                }
            }

            await base.OnDisconnectedAsync(ex);
        }

        /// <summary>
        /// REMARK: This method should ONLY be used when Agents connect, and never with Rescuers
        /// 
        /// This method is processed when an agent is connected. It checks if there are already a group for his department
        /// and if so, it he's added.
        /// Else, it creates a group for him and add any rescuer in his department who's currently online.
        /// </summary>
        private async Task<ActiveGroup> GetOrAddGroup(Department department)
        {
            var currentGroup = GetGroup(department);

            if (currentGroup == null)
            {
                var createdGroup = CreateGroup(department);

                var rescuersInDepartment = TrackerHandler.RescuerConnectionInfoSet.Where(r => r.DepartmentId == department.Id);
                foreach (var rescuer in rescuersInDepartment)
                {
                    await Groups.AddToGroupAsync(rescuer.ConnectionId, createdGroup.DepartmentName);
                    createdGroup.ActiveRescuers += 1;
                }

                return createdGroup;
            }

            return currentGroup;
        }

        /// <summary>
        /// This method is safe to be used when Rescuers connect
        /// </summary>
        private ActiveGroup GetGroup(Department department)
        {
            var currentGroup = TrackerHandler.ActiveGroups.FirstOrDefault(g => g.DepartmentName == department.ToString());

            return currentGroup;
        }

        /// <summary>
        /// Helper Function
        /// </summary>
        private ActiveGroup CreateGroup(Department department)
        {
            var newGroup = new ActiveGroup
            {
                DepartmentId = department.Id,
                DepartmentName = department.ToString()
            };

            TrackerHandler.ActiveGroups.Add(newGroup);

            return newGroup;
        }

        /// <summary>
        /// A group becomes idle if:
        ///  - No agents are currently active in the group, the group then is recycled.
        ///  - No one in the group.
        /// </summary>
        private async Task RemoveGroupIfIdle(ActiveGroup activeGroup)
        {
            var onlineAgentsInDepartment = TrackerHandler.AgentConnectionInfoSet.Count(a => a.DepartmentId == activeGroup.DepartmentId);
            if (onlineAgentsInDepartment == 0 || activeGroup.ActiveRescuers == 0)
            {
                var rescuersInDepartment = TrackerHandler.RescuerConnectionInfoSet.Where(r => r.DepartmentId == activeGroup.DepartmentId);
                foreach (var rescuer in rescuersInDepartment)
                {
                    await Groups.RemoveFromGroupAsync(rescuer.ConnectionId, activeGroup.DepartmentName);
                }

                TrackerHandler.ActiveGroups.Remove(activeGroup);
            }
        }
    }
}
