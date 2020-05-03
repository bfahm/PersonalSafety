namespace PersonalSafety.Models
{
    public class EventCategoryRepository : BaseRepository<EventCategory>, IEventCategoryRepository
    {
        private readonly AppDbContext context;

        public EventCategoryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }
    }
}
