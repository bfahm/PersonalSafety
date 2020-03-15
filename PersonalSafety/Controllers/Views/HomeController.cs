using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PersonalSafety.Controllers.Views
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        [Route("")]
        [Route("home")]
        [Route("index")]
        [Route("home/index")]
        [Route("api")]
        [Route("api/home")]
        [Route("api/home/index")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [Route("[action]")]
        public IActionResult RealtimeClient()
        {
            return View();
        }
    }
}