using PersonalSafety.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class EmergencyContactRepository : BaseRepository<EmergencyContact>, IEmergencyConactRepository
    {
        private readonly AppDbContext context;

        public EmergencyContactRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }
    }
}
