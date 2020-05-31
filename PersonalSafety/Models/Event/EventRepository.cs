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
            var events = context.Events.Include(d => d.EventCategory).Where(e => e.EventCategoryId == cateogryId).AsEnumerable();
            FilterAndOrderEvents(ref events);
            
            return events.ToList();
        }

        public List<Event> GetUserEvents(string userId)
        {
            var events = context.Events.Include(d => d.EventCategory).Where(e => e.UserId == userId).AsEnumerable();
            FilterAndOrderEvents(ref events);

            return events.ToList();
        }

        public List<Event> GetEventsByCityId(int cityId)
        {
            var events = context.Events.Include(d => d.EventCategory).Where(e => e.NearestCityId == cityId).AsEnumerable();
            FilterAndOrderEvents(ref events);

            return events.ToList();
        }

        public List<Event> GetPublicEventsByCityId(int cityId)
        {
            var events = context.Events.Include(d => d.EventCategory).Where(e => e.NearestCityId == cityId && e.IsPublicHelp).AsEnumerable();
            FilterAndOrderEvents(ref events);

            return events.ToList();
        }

        new public List<Event> GetAll()
        {
            var events = context.Events.Include(d => d.EventCategory).AsEnumerable();
            FilterAndOrderEvents(ref events);

            return events.ToList();
        }

        new public Event GetById(string eventId)
        {
            return context.Events.Include(d => d.EventCategory).FirstOrDefault(e => e.Id == int.Parse(eventId));
        }

        private void FilterAndOrderEvents(ref IEnumerable<Event> events)
        {
            events = events.Where(e => e.State == (int)StatesTypesEnum.Pending);
            events = events.OrderByDescending(r => r.CreationDate);
        }
    }
}
