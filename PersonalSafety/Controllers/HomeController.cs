using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PersonalSafety.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        [Route("")]
        [Route("api")]
        [Route("api/home")]
        [Route("api/home/index")]
        [HttpGet]
        public IActionResult Index()
        {
            //return Ok("Server Running");
            return View();
        }

        [Route("api/Home/ForgotPassword")]
        [HttpGet]
        public IActionResult ForgotPassword([FromQuery]string email, [FromQuery]string token)
        {
            return Ok(new { Email = email, Token = token });
        }
    }
}