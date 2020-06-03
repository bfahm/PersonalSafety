using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PersonalSafety.Contracts
{
    /// <remarks>
    /// Add new roles as class constants to be added automatically to the database
    /// </remarks>
    public static class Roles
    {
        public const string ROLE_ADMIN = "Admin";
        public const string ROLE_MANAGER = "Manager";
        public const string ROLE_PERSONNEL = "Personnel";
        public const string ROLE_AGENT = "Agent";
        public const string ROLE_RESCUER = "Rescuer";
        public const string ROLE_NURSE = "Nurse";

        public static List<string> GetRoles()
        {
            return typeof(Roles).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                                .Select(field => field.GetValue(null).ToString())
                                .ToList();
        }
    }
}
