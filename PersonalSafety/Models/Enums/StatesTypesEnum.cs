using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.Enums
{
    // These states are used for the two tables: [Event] and [SOSRequest]
    public enum StatesTypesEnum
    {
        Pending,
        Accepted,
        Solved,
        Canceled,
        Orphaned
    }
}
