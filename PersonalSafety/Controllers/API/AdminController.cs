﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Business;
using PersonalSafety.Contracts;
using PersonalSafety.Hubs.HubTracker;
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
        /// Retrieve full information about registered departments
        /// </summary>
        /// <remarks>
        /// ## Main Functionality
        /// Allows administrators to retrieve information about every department available in the system, including:
        /// - **Id**: Id of the department
        /// - **Location**: Longs and Lats
        /// - **City**: City Id and Name
        /// - **Authority**: Authority Id and Name
        /// - **Agents**: Emails of registered agents in the department
        /// - **Rescuers**: Emails of registered rescuers in the department
        /// 
        /// </remarks>
        [HttpGet]
        public IActionResult GetDepartments()
        {
            var authResponse = _adminBusiness.GetDepartments();

            return Ok(authResponse);
        }

        /// <summary>
        /// Create a new personnel to access web interface services.
        /// </summary>
        /// <remarks>
        /// ## Main Functionality
        /// Creates new personnel account for different departments.
        /// 
        /// 
        /// **IMPORTANT:** Only **ADMINS** are allowed to use this method and not other **personnel**.
        /// 
        /// 
        /// All these JSON object values **are** required and must follow these rules:
        /// 
        /// - **Email** : must be unique and not used before, additionally it must follow the correct email structure
        /// - **Password** : must be complex, contain number, symbols, Capital and Small letters
        /// - **FullName** : Must be non null
        ///
        /// ### Assignment To Department
        /// Registration of the agent must be through assigning him to an already registered department or a new one.
        /// Providing a department id for the field `ExistingDepartmentId` will assign the agent to that department and no need to provide these fields:
        /// - "departmentCity"
        /// - "departmentLongitude"
        /// - "departmentLatitude"
        /// - "authorityType"
        ///
        ///  
        /// Note that**AuthorityType** is an integer and can take any of the following values and **CANNOT** be a 0 value
        ///     - Police : 1
        ///     - Ambulance : 2
        ///     - Firefighting : 3
        ///     - TowTruck : 4
        ///
        /// #### Example of Registering a new Agent and assigning him to an existing department
        /// ```
        /// {
        ///     "fullName": "John Doe",
        ///     "email": "john@test.com",
        ///     "password": "fjKdl1P-mD",
        ///     "existingDepartmentId": 14
        /// }
        /// ```
        ///
        /// #### Example of Registering a new Agent and a new Department in the same request
        /// ```
        /// {
        ///     "fullName": "John Doe",
        ///     "email": "john@test.com",
        ///     "password": "fjKdl1P-mD",
        ///     "departmentCity": 0
        ///     "departmentLongitude": 30.123456012
        ///     "departmentLatitude": 30.123456012
        ///     "authorityType": 1
        /// }
        /// ```
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-1]**: Invalid Request
        /// - User exists and has registered before
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

        /// <summary>
        /// Retrieve "as is" values of the HubTracker variables
        /// </summary>
        [HttpGet]
        [Route("~/api/[controller]/Management/[action]")]
        public IActionResult RetrieveTrackers()
        {
            var response = _adminBusiness.RetrieveTrackers();
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        [HttpGet]
        [Route("~/api/[controller]/Management/[action]")]
        public IActionResult RetrieveConsole()
        {
            var response = _adminBusiness.RetrieveConsole();
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        [HttpGet]
        [Route("~/api/[controller]/Management/[action]")]
        public IActionResult ResetTrackers()
        {
            string currentlyLoggedInUserId = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            var response = _adminBusiness.ResetTrackers();
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        [HttpGet]
        [Route("~/api/[controller]/Management/[action]")]
        public IActionResult ResetConsole()
        {
            string currentlyLoggedInUserId = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            var response = _adminBusiness.ResetConsole();
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        [HttpGet]
        [Route("~/api/[controller]/Management/[action]")]
        public IActionResult ResetRescuerState([FromQuery]string rescuerEmail)
        {
            string currentlyLoggedInUserId = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            var response = _adminBusiness.ResetRescuerState(rescuerEmail);
            return Ok(response);
        }
    }
}