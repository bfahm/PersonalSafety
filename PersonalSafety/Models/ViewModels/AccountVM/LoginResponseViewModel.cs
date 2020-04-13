using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels.AccountVM
{
    public class LoginResponseViewModel
    {
        public AuthenticationDetailsViewModel AuthenticationDetails { get; set; }
        public AccountDetailsViewModel AccountDetails { get; set; }
    }

    public class AuthenticationDetailsViewModel
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }

    public class AccountDetailsViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
