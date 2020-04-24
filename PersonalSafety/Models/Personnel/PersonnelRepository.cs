using PersonalSafety.Models;
using PersonalSafety.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PersonalSafety.Models
{
    public class PersonnelRepository : BaseRepository<Personnel>, IPersonnelRepository
    {
        private readonly AppDbContext context;

        public PersonnelRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public int GetPersonnelAuthorityTypeInt(string userId)
        {
            return context.Personnels.Include(p => p.Department).FirstOrDefault(p => p.PersonnelId == userId).Department.AuthorityType;
        }

        public string GetPersonnelAuthorityTypeString(string userId)
        {
            int? authorityTypeInt = context.Personnels.Include(p => p.Department).FirstOrDefault(p => p.PersonnelId == userId)?.Department.AuthorityType;
            if (authorityTypeInt != null)
            {
                return ((AuthorityTypesEnum)authorityTypeInt).ToString();
            }
            return null;
        }

        public Department GetPersonnelDepartment(string userId)
        {
            return context.Personnels.Where(p=>p.PersonnelId == userId).Include(p => p.Department).FirstOrDefault()?.Department;
        }

        public List<string> GetDepartmentAgentsEmails(int departmentId)
        {
            return context.Personnels.Include(p => p.Department)
                .Where(p => !p.IsRescuer && p.Department.Id == departmentId).Select(p => p.ApplicationUser.Email)
                .ToList();
        }

        public List<string> GetDepartmentRescuersEmails(int departmentId)
        {
            return context.Personnels.Include(p => p.Department)
                .Where(p => p.IsRescuer && p.Department.Id == departmentId).Select(p => p.ApplicationUser.Email)
                .ToList();
        }
    }
}
