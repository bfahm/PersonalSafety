using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Hubs
{
    public interface ISOSRealtimeHelper
    {
        bool NotifyUserSOSState(int sosRequestId, int sosRequestState);
    }
}
