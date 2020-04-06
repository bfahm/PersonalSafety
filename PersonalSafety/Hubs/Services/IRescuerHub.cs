using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Hubs.Services
{
    public interface IRescuerHub
    {
        bool NotifyNewChanges(int requestId, string rescuerEmail);
        void MakeRescuerIdle(string rescuerEmail);
        bool IsIdle(string rescuerEmail);
    }
}
