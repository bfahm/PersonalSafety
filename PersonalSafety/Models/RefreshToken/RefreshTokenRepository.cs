using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
    {
        private readonly AppDbContext context;

        public RefreshTokenRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public RefreshToken GetByRefreshToken(string refreshToken)
        {
            return context.RefreshTokens.Where(x=>x.Invalidated != true && x.ExpiryDate > DateTime.Now).SingleOrDefault(x => x.Token == refreshToken);
        }

        public void RemoveUnusedTokensForUser(string userId)
        {
            var listOfUnusedTokens = context.RefreshTokens.Where(t => t.UserId == userId);
            context.RefreshTokens.RemoveRange(listOfUnusedTokens);
        }
    }
}
