using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Business;
using PersonalSafety.Models.Enums;

namespace PersonalSafety.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Personnel")]
    public class PersonnnelController : ControllerBase
    {
        private readonly IPersonnelBusiness _personnelBusiness;

        public PersonnnelController(IPersonnelBusiness personnelBusiness)
        {
            _personnelBusiness = personnelBusiness;
        }

        /// <summary>
        /// This method returns a list of Pending Requests that relates to current Personnel Authority.
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// *This method doesn't return any erros unless user is **UNAUTHORIZED***
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetPendingRequests()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _personnelBusiness.GetRelatedRequestsAsync(currentlyLoggedInUserId, (int)StatesTypesEnum.Pending);

            return Ok(response);
        }
    }
}