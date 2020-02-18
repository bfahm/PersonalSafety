using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PersonalSafety.Models;

namespace PersonalSafety.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MainController : ControllerBase
    {
        private IEmergencyConactRepository repository;

        public MainController(IEmergencyConactRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet]
        public string Index()
        {
            return "Server Running";
        }

        [HttpGet]
        public string TestRepository()
        {
            repository.Add(new EmergencyContact { Name = "test", PhoneNumber = "010", UserId = 1 });
            return repository.GetById(1).Id + "--" + repository.GetById(1).Name;
        }

        [HttpGet]
        [Route("{Id}")]
        public string TestRepositoryWithId(int Id)
        {
            return repository.GetById(Id).Name;
        }

        [HttpGet]
        public string TestRepositoryWithId2([FromQuery]int Id, [FromQuery] int Additional)
        {
            return repository.GetById(Id).Name + " and the additional was " + Additional;
        }

        [HttpGet]
        public ActionResult TestJson()
        {
            return Ok(repository.GetById(1));
        }
    }
}
