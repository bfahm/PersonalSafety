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
            return context.Personnels.Find(userId).Department.AuthorityType;
        }

        public string GetPersonnelAuthorityTypeString(string userId)
        {
            int authorityTypeInt = context.Personnels.Include(p => p.Department).FirstOrDefault(p => p.PersonnelId == userId).Department.AuthorityType;
            return ((AuthorityTypesEnum)authorityTypeInt).ToString();
        }
    }
}
