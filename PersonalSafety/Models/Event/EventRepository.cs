using PersonalSafety.Contracts.Enums;
using System.Collections.Generic;
using System.Linq;

namespace PersonalSafety.Models
{
    public class EventRepository : BaseRepository<Event>, IEventRepository
    {
        private readonly AppDbContext context;

        public EventRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public List<Event> GetFilteredEvents(int cateogryId)
        {
            return context.Events.Where(e => e.EventCategoryId == cateogryId).ToList();
        }

        public List<Event> GetUserEvents(string userId)
        {
            return context.Events.Where(e => e.UserId == userId).ToList();
        }
    }
}
