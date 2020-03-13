using PersonalSafety.Models.ViewModels;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Examples
{
    public class LoginNormalUserExample : IExamplesProvider<LoginRequestViewModel>
    {
        public LoginRequestViewModel GetExamples()
        {
            return new LoginRequestViewModel
            {
                Email = "test@test.com",
                Password = "Test@123"
            };
        }
    }
}
