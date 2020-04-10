using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Hubs
{
    public interface IAgentHub
    {
        void NotifyNewChanges(int requestId, int requestState, int departmentId);
        void NotifyChangeInRescuers(int departmentId);
    }
}
