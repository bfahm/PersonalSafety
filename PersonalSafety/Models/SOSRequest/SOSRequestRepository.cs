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

        public IEnumerable<SOSRequest> GetRelevantRequests(int authorityType, int state)
        {
            return context.SOSRequests.Where(r => r.AuthorityType == authorityType).Where(r => r.State == state);
        }
    }
}
