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
        private readonly ISOSBusiness _sosBusiness;

        public RescuerController(IRescuerBusiness rescuerBusiness, ISOSBusiness sosBusiness)
        {
            _rescuerBusiness = rescuerBusiness;
            _sosBusiness = sosBusiness;
        }

        /// <summary>
        /// Get request details by its ID.
        /// </summary>
        /// <remarks>
        ///
        /// ### Flow:
        ///
        /// - Rescuer gets a ping through the his realtime hub containing the request Id
        /// - Rescuer requests data about that request using its Id via this method.
        /// 
        /// **IMPORTANT**: Request must be accepted first by agent, and assigned to the rescuer to be able to access this data.
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
        ///
        /// ### Flow:
        ///
        /// - Rescuer uses the data retrieved from /GetSOSRequestDetails and use it to arrive to the location attached in the details
        /// - Rescuer uses this method to mark the request as Accepted and notify:
        ///     - The owner client
        ///     - His department agent
        ///
        /// **IMPORTANT**: Request must be accepted first by agent, and assigned to the rescuer.
        /// 
        /// ### Impact:
        /// This action marks the rescuer as idle again, which allows him to be assigned new requests.
        ///
        /// </remarks>
        [HttpPut]
        public async Task<IActionResult> SolveSOSRequest(int requestId)
        {
            string currentlyLoggedInUserId = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            var response = await _sosBusiness.SolveSOSRequestAsync(requestId, currentlyLoggedInUserId);

            return Ok(response);
        }
    }
}