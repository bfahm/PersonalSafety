using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Business;
using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels;
using PersonalSafety.Models.ViewModels.AdminVM;

namespace PersonalSafety.Controllers.API
{
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
        /// Retrieve "as is" values of the HubTracker variables
        /// </summary>
        [HttpGet(ApiRoutes.Admin.Technical)]
        public IActionResult RetrieveTrackers()
        {
            var response = _adminBusiness.RetrieveTrackers();
            return Ok(response);
        }

        /// <summary>
        /// Retrieve cached content of the console
        /// </summary>
        [HttpGet(ApiRoutes.Admin.Technical)]
        public IActionResult RetrieveConsole()
        {
            var response = _adminBusiness.RetrieveConsole();
            return Ok(response);
        }

        /// <summary>
        /// Reset all trackers (USE WITH CAUTION)
        /// </summary>
        [HttpPut(ApiRoutes.Admin.Technical)]
        public IActionResult ResetTrackers()
        {
            var response = _adminBusiness.ResetTrackers();
            return Ok(response);
        }

        /// <summary>
        /// Deletes console cache.
        /// </summary>
        [HttpPut(ApiRoutes.Admin.Technical)]
        public IActionResult ResetConsole()
        {
            var response = _adminBusiness.ResetConsole();
            return Ok(response);
        }

        /// <summary>
        /// Removes all occurences of the client email from the trackers
        /// </summary>
        [HttpPut(ApiRoutes.Admin.Technical)]
        public IActionResult ResetClientState([FromQuery]string clientEmail)
        {
            var response = _adminBusiness.ResetClientState(clientEmail);
            return Ok(response);
        }

        /// <summary>
        /// Removes all occurences of the rescuer  email from the trackers
        /// </summary>
        [HttpPut(ApiRoutes.Admin.Technical)]
        public IActionResult ResetRescuerState([FromQuery]string rescuerEmail)
        {
            var response = _adminBusiness.ResetRescuerState(rescuerEmail);
            return Ok(response);
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
        [HttpPost(ApiRoutes.Admin.Registration)]
        public async Task<IActionResult> RegisterAgent([FromBody] RegisterAgentViewModel request)
        {
            var authResponse = await _adminBusiness.RegisterAgentAsync(request);

            if (authResponse.HasErrors)
            {
                return BadRequest(authResponse);
            }

            return Ok(authResponse);
        }

        [HttpPost(ApiRoutes.Admin.Registration)]
        public async Task<IActionResult> RegisterManager([FromBody] RegisterManagerViewModel request)
        {
            var authResponse = await _adminBusiness.RegisterManagerAsync(request);

            if (authResponse.HasErrors)
            {
                return BadRequest(authResponse);
            }

            return Ok(authResponse);
        }

        [HttpPut(ApiRoutes.Admin.Registration)]
        public async Task<IActionResult> ModifyManagerAccess([FromBody] ModifyManagerViewModel request)
        {
            var authResponse = await _adminBusiness.ModifyManagerAccessAsync(request);

            if (authResponse.HasErrors)
            {
                return BadRequest(authResponse);
            }

            return Ok(authResponse);
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
        [HttpGet(ApiRoutes.Admin.Management)]
        public IActionResult GetDepartments()
        {
            var authResponse = _adminBusiness.GetDepartments();

            return Ok(authResponse);
        }

        /// <summary>
        /// 
        /// </summary>
        [HttpGet(ApiRoutes.Admin.Management)]
        public IActionResult GetDistributionTree()
        {
            var response = _adminBusiness.GetDistributionTree();
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        [HttpPost(ApiRoutes.Admin.Management)]
        public IActionResult AddNewDistribution([FromBody] NewDistributionRequestViewModel request)
        {
            var response = _adminBusiness.AddNewDistribution(request);
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        [HttpPost(ApiRoutes.Admin.Management)]
        public IActionResult RenameDistribution([FromBody] RenameDistributionRequestViewModel request)
        {
            var response = _adminBusiness.RenameDistribution(request);
            return Ok(response);
        }
    }
}