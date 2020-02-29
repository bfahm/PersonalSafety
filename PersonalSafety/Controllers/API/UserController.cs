﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Business.User;
using PersonalSafety.Models.ViewModels;

namespace PersonalSafety.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserBusiness _userBusiness;

        public UserController(IUserBusiness userBusiness)
        {
            _userBusiness = userBusiness;
        }

        [HttpGet]
        public IActionResult GetEmergencyContacts()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = _userBusiness.GetEmergencyContacts(currentlyLoggedInUserId);

            return Ok(response);
        }

        /// <summary>
        /// This method updates the basic information needed for the currently logged in user.
        /// </summary>
        /// <remarks>
        /// # **`AuthenticatedRequest`**
        /// ## Main Functionality
        /// End-user can update his:
        /// - Current Address
        /// - Blood Type
        ///     -  O = 1
        ///     -  A = 2
        ///     -  B = 3
        ///     -  AB = 4
        /// - Medical Notes (and history)
        /// 
        /// #### **IMPORTANT:** These fields are not mandatory, and not providing any values for them will result in the fields not updated. So users can only provide data to which they want to update.
        /// 
        /// User could also modify his current Emergency Contacts **by providing new ones here**.
        /// ####**IMPORTANT:** New emergency contacts replace old ones, so there is no need for separate delete/update calls for the sake of simplicity. Just provide the new setup and it will replace the old one.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-2]**: IdentityError
        /// This is a generic error code resembles something went wrong inside the Identity Framework and can be diagnosed using the response Messages list.
        /// #### **[-4]**: NotConfrimed
        /// Could happen if the email matching the provided token was not verified.
        /// #### **[401]**: Unauthorized
        /// Could happen if the provided token in the header has expired or is not valid.
        /// </remarks>
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileViewModel request)
        {
            //? means : If value is not null, retrieve it
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var response = await _userBusiness.CompleteProfileAsync(currentlyLoggedInUserId, request);

            return Ok(response);
        }
    }
}