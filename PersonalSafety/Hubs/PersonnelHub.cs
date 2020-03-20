using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PersonalSafety.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PersonalSafety.Hubs
{
    [Authorize(Roles ="Personnel")]
    public class PersonnelHub : MainHub, IPersonnelHub
    {
        private readonly IHubContext<PersonnelHub> _hubContext;
        private readonly string channelName = "PersonnelChannel";

        public PersonnelHub(IHubContext<PersonnelHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task NotifyNewChanges(int requestId, int requestState)
        {
            var jsonMsg = JsonSerializer.Serialize(new { requestId = requestId, requestState = ((StatesTypesEnum)requestState).ToString() });
            
            return _hubContext.Clients.All.SendAsync(channelName, jsonMsg);
        }
    }
}
