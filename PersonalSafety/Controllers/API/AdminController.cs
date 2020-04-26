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

        /// <summary>
        /// Create a new manager to have an elevated access to different department.
        /// </summary>
        /// <remarks>
        /// ## Main Functionality
        /// Creates new manager account that have access to a group of departments.
        /// 
        /// 
        /// **IMPORTANT:** Only **ADMINS** are allowed to use this method.
        /// 
        /// 
        /// All these JSON object values **are** required and must follow these rules:
        /// 
        /// - **Email** : must be unique and not used before, additionally it must follow the correct email structure
        /// - **Password** : must be complex, contain number, symbols, Capital and Small letters
        /// - **FullName** : Must be non null
        /// - **DistributionId** : An integer that represents the parent node and childs in which the manager would have access to
        ///     - Consult `/Management/GetDistributionTree` to figure out which distribution the manager should be assigned to.
        ///
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// #### **[-1]**: Invalid Request
        /// - User exists and has registered before
        /// - Invalid AuthorityType
        /// 
        /// #### **[-2]**: Identity Error
        /// This is a generic error code resembles something went wrong inside the Identity Framework and can be diagnosed using the response Messages list.
        /// 
        /// #### **[404]**: Not Found Error
        /// Occurs when you provide a wrong department Id.
        /// </remarks>
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

        /// <summary>
        /// Modify permissions for existing managers
        /// </summary>
        /// <remarks>
        /// ## Main Functionality
        /// Modifies the group of departments a manager has access to.
        /// 
        /// 
        /// **IMPORTANT:** Only **ADMINS** are allowed to use this method.
        /// 
        /// 
        /// All these JSON object values **are** required and must follow these rules:
        /// 
        /// - **Email** : an email representing the existing account
        /// - **DistributionId** : An integer that represents the parent node and childs in which the manager would have access to
        ///     - Consult `/Management/GetDistributionTree` to figure out which distribution the manager should be assigned to.
        ///
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// 
        /// #### **[-2]**: Identity Error
        /// This is a generic error code resembles something went wrong inside the Identity Framework and can be diagnosed using the response Messages list.
        /// 
        /// #### **[404]**: Not Found Error
        /// Occurs when you provide a wrong email address or department Id.
        /// </remarks>
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
        /// Get the tree of department distribution.
        /// </summary>
        /// <remarks>
        /// ## Main Functionality
        /// Retrieves a tree that is filled recursively representing the full graph of the department distribution in the system. 
        /// </remarks>
        [HttpGet(ApiRoutes.Admin.Management)]
        public IActionResult GetDistributionTree()
        {
            var response = _adminBusiness.GetDistributionTree();
            return Ok(response);
        }

        /// <summary>
        /// Attachs a new node to the Distribution Tree
        /// </summary>
        /// <remarks>
        /// All these JSON object values **are** required and must follow these rules:
        /// 
        /// - **parentId** : the id of the parent where the new node would be attached under
        /// - **value** : a string representing the value of the node
        /// 
        /// 
        /// 
        /// **IMPORTANT:** `DistributionType` is automatically calculated by calculating the number of direct parents, 
        /// no need to provide it.
        ///
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// 
        /// #### **[404]**: Not Found Error
        /// Occurs when you provide a wrong parent id.
        /// </remarks>
        [HttpPost(ApiRoutes.Admin.Management)]
        public IActionResult AddNewDistribution([FromBody] NewDistributionRequestViewModel request)
        {
            var response = _adminBusiness.AddNewDistribution(request);
            return Ok(response);
        }

        /// <summary>
        /// Renames an existing node in the Distribution Tree
        /// </summary>
        /// <remarks>
        /// All these JSON object values **are** required and must follow these rules:
        /// 
        /// - **id** : the id of the node to be renamed
        /// - **value** : new string value for the node
        /// 
        ///         
        ///
        /// 
        /// ## Possible Result Codes in case of Errors:
        /// 
        /// #### **[404]**: Not Found Error
        /// Occurs when you provide a wrong node id.
        /// </remarks>
        [HttpPost(ApiRoutes.Admin.Management)]
        public IActionResult RenameDistribution([FromBody] RenameDistributionRequestViewModel request)
        {
            var response = _adminBusiness.RenameDistribution(request);
            return Ok(response);
        }
    }
}