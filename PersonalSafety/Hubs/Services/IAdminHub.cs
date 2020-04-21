using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Hubs
{
    public interface IAdminHub
    {
        Task NotifyChanges(string channelName);
    }
}
