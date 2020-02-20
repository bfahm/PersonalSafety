using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Helpers;
using PersonalSafety.Models;
using PersonalSafety.Models.ViewModels;

namespace PersonalSafety.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }


        [HttpPost]
        public async Task<object> Register(RegisterViewModel model)
        {
            APIResult<string> result = new APIResult<string>();

            // Copy data from RegisterViewModel to ApplicationUser
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            // Store user data in AspNetUsers database table
            var registrationResult = await userManager.CreateAsync(user, model.Password);

            // If there are any errors, add them to the ModelState object
            // which will be displayed by the validation summary tag helper
            foreach (var error in registrationResult.Errors)
            {
                result.Message += " " + error.Description;
            }

            //TODO: Fix status codes later
            result.Status = registrationResult.Succeeded == true ? 1 : 0;

            return result;
        }
    }
}