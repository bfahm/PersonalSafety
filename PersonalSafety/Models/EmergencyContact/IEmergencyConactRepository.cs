using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public interface IEmergencyContactRepository : IBaseRepository<EmergencyContact>
    {
        IEnumerable<EmergencyContact> GetByUserId(string userId);
    }
}
