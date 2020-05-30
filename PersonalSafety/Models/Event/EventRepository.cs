using Microsoft.EntityFrameworkCore;
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
            return context.Events.Include(d => d.EventCategory).Where(e => e.EventCategoryId == cateogryId).ToList();
        }

        public List<Event> GetUserEvents(string userId)
        {
            return context.Events.Include(d => d.EventCategory).Where(e => e.UserId == userId).ToList();
        }

        public List<Event> GetEventsByCityId(int cityId)
        {
            return context.Events.Include(d => d.EventCategory).Where(e => e.NearestCityId == cityId).ToList();
        }

        public List<Event> GetPublicEventsByCityId(int cityId)
        {
            return context.Events.Include(d => d.EventCategory).Where(e => e.NearestCityId == cityId && e.IsPublicHelp).ToList();
        }

        new public Event GetById(string eventId)
        {
            return context.Events.Include(d => d.EventCategory).FirstOrDefault(e => e.Id == int.Parse(eventId));
        }

        new public List<Event> GetAll()
        {
            return context.Events.Include(d => d.EventCategory).ToList();
        }
    }
}
