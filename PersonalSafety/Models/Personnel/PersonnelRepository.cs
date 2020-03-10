using PersonalSafety.Models;
using PersonalSafety.Models.Enums;
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

        public int GetPersonnelAuthorityTypeInt(string userId)
        {
            return context.Personnels.Find(userId).AuthorityType;
        }

        public string GetPersonnelAuthorityTypeString(string userId)
        {
            int authorityTypeInt = context.Personnels.Find(userId).AuthorityType;
            return ((AuthorityTypesEnum)authorityTypeInt).ToString();
        }
    }
}
