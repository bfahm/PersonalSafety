using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class DepartmentRepository: BaseRepository<Department>, IDepartmentRepository
    {
        private readonly AppDbContext context;
        private readonly IDistributionRepository _distributionRepository;

        public DepartmentRepository(AppDbContext context, IDistributionRepository distributionRepository) : base(context)
        {
            this.context = context;
            _distributionRepository = distributionRepository;
        }

        public IEnumerable<Department> GetDepartmentsByAuthority(int authorityType)
        {
            return context.Departments.Where(d => d.AuthorityType == authorityType);
        }

        public new IEnumerable<Department> GetAll()
        {
            return context.Departments.Include(d => d.Distribution);
        }

        public new Department GetById(string Id)
        {
            return context.Departments.Include(d=>d.Distribution).SingleOrDefault(d=>d.Id == int.Parse(Id));
        }

        public IEnumerable<Department> GetByCity(int distributionId)
        {
            if (_distributionRepository.IsCity(distributionId))
            {
                return context.Departments.Include(d => d.Distribution).Where(d => d.DistributionId == distributionId);
            }
            return new List<Department>();
        }

        public List<Department> GetAll(int distributionFilter)
        {
            var allowedCities = _distributionRepository.GetGrantedDistributions(distributionFilter);
            var returnDepartments = new List<Department>();

            foreach(var city in allowedCities)
            {
                returnDepartments.AddRange(GetByCity(city.Id));
            }

            return returnDepartments;
        }
    }
}
