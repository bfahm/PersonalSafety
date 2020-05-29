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
    [Authorize(Roles = Roles.ROLE_ADMIN + "," + Roles.ROLE_MANAGER + "," + Roles.ROLE_AGENT + "," + Roles.ROLE_RESCUER)]
    public class LocationHub : Hub
    {
        private readonly IPersonnelRepository _personnelRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly string locationChannelName = "LocationChannel";
        private readonly string infoChannelName = "InfoChannel";
        private readonly string alertsChannelName = "AlertsChannel";

        public LocationHub(IPersonnelRepository personnelRepository, IDepartmentRepository departmentRepository)
        {
            _personnelRepository = personnelRepository;
            _departmentRepository = departmentRepository;
        }

        public Task ShareLocation(string departmentName, string rescuerEmail, double latitude, double longitude)
        {
            return Clients.Group(departmentName).SendAsync(locationChannelName, rescuerEmail, latitude, longitude);
        }

        public async Task EnterDepartmentRoom(string signalRGroupName = null)
        {
            var userId = Context.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            var isAdmin = Context.User.Claims.FirstOrDefault(x => x.Value == Roles.ROLE_ADMIN) != null;
            var isManager = Context.User.Claims.FirstOrDefault(x => x.Value == Roles.ROLE_MANAGER) != null;
            var isAgent = Context.User.Claims.FirstOrDefault(x => x.Value == Roles.ROLE_AGENT) != null;
            var isRescuer = Context.User.Claims.FirstOrDefault(x => x.Value == Roles.ROLE_RESCUER) != null;

            string departmentName;

            if (signalRGroupName != null && signalRGroupName.Length > 0 && (isAdmin || isManager))
            {
                var exisitngDpt = _departmentRepository.GetAll().FirstOrDefault(d => d.ToString() == signalRGroupName);
                if(exisitngDpt == null)
                {
                    await Clients.Caller.SendAsync(infoChannelName, "Error: The requested department was not found.");
                    return;
                }

                departmentName = signalRGroupName;
            }
            else if (isAgent || isRescuer)
                departmentName = _personnelRepository.GetPersonnelDepartment(userId).ToString();
            else
            {
                await Clients.Caller.SendAsync(infoChannelName, "Error: You must provide a valid Department Identitifier..");
                return;
            }
                

            if (!isRescuer)
            {
                var departmentChatGroup = GetOrAddGroup(departmentName);

                await Groups.AddToGroupAsync(Context.ConnectionId, departmentChatGroup.DepartmentName);
                await Clients.Group(departmentChatGroup.DepartmentName).SendAsync(alertsChannelName, $"{userId} / MONITOR JOINED!");

                await Clients.Caller.SendAsync(infoChannelName, departmentChatGroup.DepartmentName);
            }
            else
            {
                var departmentChatGroup = GetOrAddGroup(departmentName);
                
                departmentChatGroup.ActiveRescuers += 1;

                await Groups.AddToGroupAsync(Context.ConnectionId, departmentChatGroup.DepartmentName);
                await Clients.Group(departmentChatGroup.DepartmentName).SendAsync(alertsChannelName, $"{userId} / RESUCER  JOINED!");

                await Clients.Caller.SendAsync(infoChannelName, departmentName);
            }
        }

        public override async Task OnConnectedAsync()
        {
            var isAgent = Context.User.Claims.FirstOrDefault(x => x.Value == Roles.ROLE_AGENT) != null;
            var isRescuer = Context.User.Claims.FirstOrDefault(x => x.Value == Roles.ROLE_RESCUER) != null;

            if (isAgent || isRescuer)
                await EnterDepartmentRoom();
            else
                await Clients.Caller.SendAsync(infoChannelName, "It appears you are an Admin / Manager. Join a department room using EnterDepartmentRoom() and pass the name of the group you want to join.");

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
                var currentGroup = GetOrAddGroup(department.ToString());

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, currentGroup?.DepartmentName);

                if (currentGroup != null && isRescuer)
                {
                    currentGroup.ActiveRescuers -= 1;

                    RecyclerGroup(currentGroup);

                    await Clients.Group(currentGroup.DepartmentName).SendAsync(alertsChannelName, $"{userId} / RESUCER  DISCONNECTED!");
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
        public ActiveGroup GetOrAddGroup(string departmentName)
        {
            var currentGroup = TrackerHandler.ActiveGroups.FirstOrDefault(g => g.DepartmentName == departmentName);

            if (currentGroup == null)
            {
                currentGroup = new ActiveGroup
                {
                    DepartmentName = departmentName
                };

                TrackerHandler.ActiveGroups.Add(currentGroup);
            }

            return currentGroup;
        }

        private void RecyclerGroup(ActiveGroup group)
        {
            if(group.ActiveRescuers <= 0)
            {
                TrackerHandler.ActiveGroups.RemoveWhere(g => g.DepartmentName == group.DepartmentName);
            }
        }
    }
}
