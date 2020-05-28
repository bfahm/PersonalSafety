using PersonalSafety.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public interface IEventRepository : IBaseRepository<Event>
    {
        List<Event> GetFilteredEvents(string userId, EventFiltersEnum filter, int? cateogryId);
    }
}
