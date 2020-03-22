using PersonalSafety.Contracts;
using PersonalSafety.Helpers;
using PersonalSafety.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Business
{
    public interface ISOSBusiness
    {
        APIResponse<bool> UpdateSOSRequest(int requestId, int newStatus);
    }
}
