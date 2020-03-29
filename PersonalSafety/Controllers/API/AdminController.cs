﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Business;
using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels;

namespace PersonalSafety.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = Roles.ROLE_ADMIN)]
    public class AdminController : ControllerBase
    {
        private readonly IAdminBusiness _adminBusiness;

        public AdminController(IAdminBusiness adminBusiness)
        {
            _adminBusiness = adminBusiness;
        }

        /// <summary>
        /// Create a new personnel to access web interface services.
        /// </summary>
        /// <remarks>
        /// ## Main Functionality
        /// Creates new personnel account for different departments.
        /// 
        /// 
        /// **IMPORTANT:** Only **ADMINS** are alowed to use this method and not other **personnel**.
        /// 
        /// 
        /// All the JSON object values **are** required and must follow these rules:
        /// 
        /// - **Email** : must be unique and not used before, additionally it must follow the correct email structure
        /// - **Password** : must be complex, contain number, symbols, Capital and Small letters
        /// - **FullName** : Must be non null
        /// - **AuthorityType**: is an integer and can take any of the following values and **CANNOT** be a 0 value
        ///     - Police : 1
        ///     - Ambulance : 2
        ///     - Firefighting : 3
        ///     - TowTruck : 4
        /// 
        /// After a valid attempt, this function also **automatically** sends a verification email to be used in `/api/Account/ConfirmMail` directly.
        /// **IMPORTANT:** User does not have access to any of the system's functionality till he actually verify his email.
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-1]**: Invalid Request
        /// - User exsists and has registered before
        /// - Invalid AuthorityType
        /// 
        /// #### **[-2]**: Identity Error
        /// This is a generic error code resembles something went wrong inside the Identity Framework and can be diagnosed using the response Messages list.
        /// </remarks>
        [HttpPost]
        public async Task<IActionResult> RegisterAgent([FromBody] RegisterAgentViewModel request)
        {
            var authResponse = await _adminBusiness.RegisterAgentAsync(request);

            if (authResponse.HasErrors)
            {
                return BadRequest(authResponse);
            }

            return Ok(authResponse);
        }
    }
}