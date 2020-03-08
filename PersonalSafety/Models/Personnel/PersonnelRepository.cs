using PersonalSafety.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class PersonnelRepository : BaseRepository<Personnel>, IPersonnelRepository
    {
        private readonly AppDbContext context;

        public PersonnelRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }
    }
}
