using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Hubs
{
    public interface IClientHub : IMainHub
    {
        bool NotifyUserSOSState(int sosRequestId, int sosRequestState);
        bool TrackSOSIdForClient(string clientEmail, int sosRequestId);
        void RemoveClientFromTrackers(string userId);
    }
}
