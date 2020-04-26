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
    [Route(ApiRoutes.Default)]
    [ApiController]
    [Authorize(Roles = Roles.ROLE_ADMIN + "," + Roles.ROLE_MANAGER)]
    public class ManagerController : ControllerBase
    {
        private readonly IManagerBusiness _managerBusiness;

        public ManagerController(IManagerBusiness managerBusiness)
        {
            _managerBusiness = managerBusiness;
        }

        /// <summary>
        /// Retrieve full information about registered departments
        /// </summary>
        /// <remarks>
        /// ## Main Functionality
        /// Allows managers to retrieve information about department in their granted distribution, including:
        /// - **Id**: Id of the department
        /// - **Location**: Longs and Lats
        /// - **Distribution** details
        /// - **City**: City Id and Name
        /// - **Authority**: Authority Id and Name
        /// - **Agents**: Emails of registered agents in the department
        /// - **Rescuers**: Emails of registered rescuers in the department
        /// 
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;
            var authResponse = await _managerBusiness.GetDepartmentsAsync(currentlyLoggedInUserId);

            return Ok(authResponse);
        }

        /// <summary>
        /// Retrieve full list of request in a particular department
        /// </summary>
        /// <remarks>
        /// **IMPORTANT:** the provided departmentId **MUST** belong to the list of departments that are allowed to the manager
        /// 
        /// #### **[401]**: Unauthorized Error
        /// Occurs when you provide a department Id that doesn't belong to your distribution.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetDepartmentRequests([FromQuery] int departmentId)
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;
            var authResponse = await _managerBusiness.GetDepartmentRequestsAsync(currentlyLoggedInUserId, departmentId, null, true);

            return Ok(authResponse);
        }
    }
}