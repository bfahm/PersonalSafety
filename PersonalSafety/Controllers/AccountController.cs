using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Models.ViewModels;
using PersonalSafety.Services;

namespace PersonalSafety.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountBusiness _identityService;

        public AccountController(IAccountBusiness identityService)
        {
            _identityService = identityService;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestViewModel request)
        {
            var authResponse = await _identityService.RegisterAsync(request);

            if (authResponse.HasErrors)
            {
                return BadRequest(authResponse);
            }

            return Ok(authResponse);
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequestViewModel request)
        {
            var authResponse = await _identityService.LoginAsync(request);

            if (authResponse.HasErrors)
            {
                return BadRequest(authResponse);
            }

            return Ok(authResponse);
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword([FromQuery] string mail)
        {
            var authResponse = await _identityService.ForgotPasswordAsync(mail);

            return Ok(authResponse);
        }


    }
}