﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public interface IClientTrackingRepository : IBaseRepository<ClientTracking>
    {
        List<string> GetSusceptibleAccounts(string victimUserId);

        // Adminstrative
        void SetMinutesSkew(int newValue);
        int GetMinutesSkew();
        void SetMetersSkew(int newValue);
        int GetMetersSkew();
    }
}
