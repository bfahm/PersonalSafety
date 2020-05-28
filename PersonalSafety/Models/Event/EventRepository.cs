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

        public List<Event> GetFilteredEvents(string userId, EventFiltersEnum filter, int? cateogryId)
        {
            switch (filter)
            {
                case EventFiltersEnum.ALL_EVENTS:
                    return GetAll().ToList();

                case EventFiltersEnum.USER_EVENTS:
                    return GetAll().Where(e => e.UserId == userId).ToList();

                default:
                    return GetAll().Where(e => e.EventCategoryId == cateogryId).ToList();
            }
        }
    }
}
