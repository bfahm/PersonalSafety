using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Business;
using PersonalSafety.Contracts;

namespace PersonalSafety.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = Roles.ROLE_RESCUER)]
    public class RescuerController : ControllerBase
    {
        private readonly IRescuerBusiness _rescuerBusiness;

        public RescuerController(IRescuerBusiness rescuerBusiness)
        {
            _rescuerBusiness = rescuerBusiness;
        }

        /// <summary>
        /// Get request details by its ID.
        /// </summary>
        /// <remarks>
        /// **IMPORTANT**: Request must be accepted first by agent, and assigned to the rescuer.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetSOSRequestDetails(int requestId)
        {
            string currentlyLoggedInUserId = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            var response = await _rescuerBusiness.GetSOSRequestDetailsAsync(currentlyLoggedInUserId, requestId);

            return Ok(response);
        }

        /// <summary>
        /// Mark a request as Solved.
        /// </summary>
        /// <remarks>
        /// **IMPORTANT**: Request must be accepted first by agent, and assigned to the rescuer.
        /// </remarks>
        [HttpPut]
        public async Task<IActionResult> SolveSOSRequest(int requestId)
        {
            string currentlyLoggedInUserId = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            var response = await _rescuerBusiness.SolveSOSRequestAsync(currentlyLoggedInUserId, requestId);

            return Ok(response);
        }
    }
}