using PersonalSafety.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Business
{
    public interface ISOSBusiness
    {
        Task<APIResponse<bool>> UpdateSOSRequestAsync(int requestId, int newStatus, string issuerId, string rescuerEmail = null);
    }
}
