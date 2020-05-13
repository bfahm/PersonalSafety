using System.Collections.Generic;
using System.Linq;

namespace PersonalSafety.Models
{
    public class EmergencyContactRepository : BaseRepository<EmergencyContact>, IEmergencyContactRepository
    {
        private readonly AppDbContext context;

        public EmergencyContactRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public IEnumerable<EmergencyContact> GetByUserId(string userId)
        {
            return context.EmergencyContacts.Where(u => u.UserId== userId);
        }

        public void DeleteForUser(string userId)
        {
            IEnumerable<EmergencyContact> emergencyContactsForUser = GetByUserId(userId);
            context.RemoveRange(emergencyContactsForUser);
        }
    }
}
