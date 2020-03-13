using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.Enums
{
    enum APIResponseCodesEnum
    {
        Ok = 0,
        InvalidRequest = -1,
        IdentityError = -2,
        TechnicalError = -3,
        NotConfirmed = -4,
        SignalRError = -5,
        BadRequest = 400, // Happens when validation error occur 
        Unauthorized = 401,
        NotFound = 404
    }
}
