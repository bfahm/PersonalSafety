using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public interface IPersonnelRepository : IBaseRepository<Personnel>
    {
        int GetPersonnelAuthorityTypeInt(string userId);
        string GetPersonnelAuthorityTypeString(string userId);
    }
}
