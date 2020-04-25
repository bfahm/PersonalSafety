using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public interface IDepartmentRepository : IBaseRepository<Department>
    {
        IEnumerable<Department> GetDepartmentsByAuthority(int authorityType);
        new IEnumerable<Department> GetAll();
        new Department GetById(string Id);
    }
}
