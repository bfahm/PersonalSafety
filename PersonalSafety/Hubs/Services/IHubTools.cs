using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Hubs
{
    public interface IHubTools
    {
        void PrintToConsole(string email, string connectionId, bool hasDisconnected);

        void PrintToConsole(string email, string customText);

        void PrintToConsole(string customText);
    }
}
