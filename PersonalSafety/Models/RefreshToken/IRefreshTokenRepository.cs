using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
    {
        RefreshToken GetByRefreshToken(string refreshToken);
        void RemoveUnusedTokensForUser(string userId);
    }
}
