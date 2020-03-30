using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Hubs
{
    public interface IAgentHub
    {
        Task NotifyNewChanges(int requestId, int requestState);
    }
}
