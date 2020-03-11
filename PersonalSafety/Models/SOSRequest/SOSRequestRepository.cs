using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public IEnumerable<SOSRequest> GetRelevantRequests(int authorityType)
        {
            return context.SOSRequests.Where(r => r.AuthorityType == authorityType);
        }

        // Filter them depending on their state
        public IEnumerable<SOSRequest> GetRelevantRequests(int authorityType, int state)
        {
            return context.SOSRequests.Where(r => r.AuthorityType == authorityType).Where(r => r.State == state);
        }
    }
}
