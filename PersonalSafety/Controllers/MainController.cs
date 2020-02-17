using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PersonalSafety.Controllers
{
    [ApiController]
    [Route("api/{controller}")]
    public class MainController : ControllerBase
    {
        [HttpGet]
        public string Index()
        {
            return "Server Running";
        }
    }
}
