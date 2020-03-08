using PersonalSafety.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class ClientRepository : BaseRepository<Client>, IClientRepository
    {
        private readonly AppDbContext context;

        public ClientRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }
    }
}
