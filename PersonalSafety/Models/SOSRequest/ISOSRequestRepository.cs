using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public interface ISOSRequestRepository : IBaseRepository<SOSRequest>
    {
        IEnumerable<SOSRequest> GetRelevantRequests(int authorityType, int dptId);
        IEnumerable<SOSRequest> GetRelevantRequests(int authorityType, int dptId, int state);
        bool UserHasOngoingRequest(string userId);
        IEnumerable<SOSRequest> GetOngoingRequest(string userId);
    }
}
