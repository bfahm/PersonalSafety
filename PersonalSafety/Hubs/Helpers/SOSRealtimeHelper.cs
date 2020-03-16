using Microsoft.AspNetCore.SignalR;
using PersonalSafety.Hubs.HubTracker;
using PersonalSafety.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Hubs.Helpers
{
    public class SOSRealtimeHelper: ISOSRealtimeHelper
    {
        private readonly string channelName = "ReceiveMessage";
        private readonly IHubContext<MainHub> _hubContext;

        public SOSRealtimeHelper(IHubContext<MainHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// True if user had a maintained connection with the server, else false
        /// </returns>
        public bool NotifyUserSOSState(int sosRequestId, int sosRequestState)
        {
            SOSInfo connectionInformation = SOSHandler.SOSInfoSet.Where(r => r.SOSId == sosRequestId).FirstOrDefault();

            if (connectionInformation != null)
            {
                _hubContext.Clients.Client(connectionInformation.ConnectionId).SendAsync(channelName, "Your request with id " + sosRequestId + " was " + ((StatesTypesEnum)sosRequestState).ToString() + ".");

                return true;
            }
            return false;
        }
    }
}
