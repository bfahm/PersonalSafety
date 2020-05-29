using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace PersonalSafety.Models
{
    public class ClientRepository : BaseRepository<Client>, IClientRepository
    {
        private readonly AppDbContext context;

        public ClientRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public Client GetByNationalId(string nationalId)
        {
            return context.Clients.FirstOrDefault(u => u.NationalId == nationalId);
        }

        new public Client GetById(string Id)
        {
            return context.Clients.Include(c => c.ApplicationUser).FirstOrDefault(c => c.ClientId == Id);
        }

        public IEnumerable<Client> GetClientsByCityId(int cityId)
        {
            return context.Clients.Where(c => c.LastKnownCityId == cityId);
        }
    }
}
