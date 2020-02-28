﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PersonalSafety.Models;

namespace PersonalSafety.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TestController : ControllerBase
    {
        //To be removed, using [EmergencyContact] table as a testing one
        private IEmergencyContactRepository repository;
        public TestController(IEmergencyContactRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet]
        public string TestRepository()
        {
            //repository.Add(new EmergencyContact { Name = "test", PhoneNumber = "010", UserId = 1 });
            int lastAddedId = repository.GetAll().ToList().Count;
            return repository.GetById(lastAddedId).Id + "--" + repository.GetById(lastAddedId).Name;
        }

        [HttpGet]
        [Route("{Id}")]
        public string TestRepositoryWithId(int Id)
        {
            return repository.GetById(Id).Id + "--" + repository.GetById(Id).Name;
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
