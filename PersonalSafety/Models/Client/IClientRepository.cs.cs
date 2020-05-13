namespace PersonalSafety.Models
{
    public interface IClientRepository : IBaseRepository<Client>
    {
        Client GetByNationalId(string nationalId);
        new Client GetById(string Id);
    }
}
