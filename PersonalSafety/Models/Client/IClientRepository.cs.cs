using System.Collections.Generic;

namespace PersonalSafety.Models
{
    public interface IClientRepository : IBaseRepository<Client>
    {
        Client GetByNationalId(string nationalId);
        new Client GetById(string Id);
        IEnumerable<Client> GetClientsByCityId(int cityId);
    }
}
