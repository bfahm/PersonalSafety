using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PersonalSafety.Contracts.Enums;

namespace PersonalSafety.Models
{
    public class SOSRequestRepository : BaseRepository<SOSRequest>, ISOSRequestRepository
    {
        private readonly AppDbContext context;

        public SOSRequestRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        // Get all requests for the current personnel
        public IEnumerable<SOSRequest> GetRelevantRequests(int authorityType, int dptId)
        {
            var requests = context.SOSRequests.Where(r => r.AuthorityType == authorityType && r.AssignedDepartmentId == dptId).AsEnumerable();
            OrderRequests(ref requests);

            return requests;
        }

        // Filter them depending on their state
        public IEnumerable<SOSRequest> GetRelevantRequests(int authorityType, int dptId, int state)
        {
            var requests = context.SOSRequests.Where(r =>
                r.AuthorityType == authorityType && r.AssignedDepartmentId == dptId && r.State == state).AsEnumerable();
            OrderRequests(ref requests);

            return requests;
        }

        // Get all requests in any of these department Ids
        public IEnumerable<SOSRequest> GetRequestsInDepartments(List<int> dptIds)
        {
            return context.SOSRequests.Where(r => dptIds.Contains(r.AssignedDepartmentId));
        }

        private void OrderRequests(ref IEnumerable<SOSRequest> requests)
        {
            requests = requests.OrderBy(r => r.State).ThenBy(r => r.CreationDate);
        }

        public bool UserHasOngoingRequest(string userId)
        {
            return GetOngoingRequest(userId).Any();
        }

        public IEnumerable<SOSRequest> GetOngoingRequest(string userId)
        {
            var sosRequestForGivenClient = context.SOSRequests.Where(s => s.UserId == userId).ToList();
            return sosRequestForGivenClient.Where(s => s.State == (int)StatesTypesEnum.Pending || s.State == (int)StatesTypesEnum.Accepted);
        }
    }
}
