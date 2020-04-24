using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models.ViewModels.AccountVM
{
    public class RefreshTokenRequestViewModel
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
