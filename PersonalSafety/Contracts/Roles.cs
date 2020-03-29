using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Contracts
{
    /// <remarks>
    /// Add new roles as class constants and also don't forget to add them to the return list of GetRoles()..
    /// </remarks>
    public class Roles
    {
        public const string ROLE_ADMIN = "Admin";
        public const string ROLE_PERSONNEL = "Personnel";
        public const string ROLE_AGENT = "Agent";
        public const string ROLE_RESCUER = "Rescuer";

        public static List<string> GetRoles()
        {
            return new List<string>
            {
                ROLE_ADMIN, ROLE_PERSONNEL, ROLE_AGENT, ROLE_RESCUER
            };
        }
    }
}
