using PersonalSafety.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public interface IEventRepository : IBaseRepository<Event>
    {
        List<Event> GetFilteredEvents(int cateogryId);
        List<Event> GetUserEvents(string userId);
        new Event GetById(string eventId);
    }
}
