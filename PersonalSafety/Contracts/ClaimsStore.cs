using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PersonalSafety.Contracts
{
    /// <remarks>
    /// Add new claims as class constants to be added automatically to the database
    /// </remarks>
    public static class ClaimsStore
    {
        public const string CLAIM_DISTRIBUTION_ACCESS = "DistributionAccess";

        public static List<string> GetClaims()
        {
            return typeof(Roles).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                                .Select(field => field.GetValue(null).ToString())
                                .ToList();
        }
    }
}
