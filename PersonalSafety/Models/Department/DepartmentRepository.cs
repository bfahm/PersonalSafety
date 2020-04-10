using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class DepartmentRepository: BaseRepository<Department>, IDepartmentRepository
    {
        private readonly AppDbContext context;

        public DepartmentRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public IEnumerable<Department> GetDepartmentsByAuthority(int authorityType)
        {
            return context.Departments.Where(d => d.AuthorityType == authorityType);
        }
    }
}
