using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PersonalSafety.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Server Running");
        }

        [HttpGet]
        public IActionResult ForgotPassword([FromQuery]string email, [FromQuery]string token)
        {
            return Ok(new { Email = email, Token = token});
        }
    }
}