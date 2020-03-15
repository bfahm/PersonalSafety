using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PersonalSafety.Controllers.Views
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("admin")]
    [Route("admin/[action]")]
    public class AdminVisualController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}