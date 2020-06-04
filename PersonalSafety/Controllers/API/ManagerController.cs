using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalSafety.Business;
using PersonalSafety.Contracts;
using PersonalSafety.Models.ViewModels;

namespace PersonalSafety.Controllers.API
{
    [ApiController]
    [Authorize(Roles = Roles.ROLE_ADMIN + "," + Roles.ROLE_MANAGER)]
    public class ManagerController : ControllerBase
    {
        private readonly IManagerBusiness _managerBusiness;
        private readonly ICategoryBusiness _categoryBusiness;
        private readonly IEventsBusiness _eventsBusiness;

        public ManagerController(IManagerBusiness managerBusiness, ICategoryBusiness categoryBusiness, IEventsBusiness eventsBusiness)
        {
            _managerBusiness = managerBusiness;
            _categoryBusiness = categoryBusiness;
            _eventsBusiness = eventsBusiness;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpGet(ApiRoutes.Manager.Stats)]
        public async Task<IActionResult> GetTopCardsData()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var authResponse = await _managerBusiness.GetTopCardsDataAsync(currentlyLoggedInUserId);

            return Ok(authResponse);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpGet(ApiRoutes.Manager.Stats)]
        public async Task<IActionResult> GetSOSChartData()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var authResponse = await _managerBusiness.GetSOSChartDataAsync(currentlyLoggedInUserId);

            return Ok(authResponse);
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
        [HttpGet(ApiRoutes.Manager.Departments)]
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
        [HttpGet(ApiRoutes.Manager.Departments)]
        public async Task<IActionResult> GetDepartmentRequests([FromQuery] int departmentId)
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;
            var authResponse = await _managerBusiness.GetDepartmentRequestsAsync(currentlyLoggedInUserId, departmentId, null, true);

            return Ok(authResponse);
        }

        /// <summary>
        /// Gets all Categories of Events along there thumbnails (if available)
        /// </summary>
        /// <remarks>
        /// ### Remarks:
        /// Please note that access to the thumbnail contents also requires user authorization via a normal get request
        /// with the `Bearer` token attached.
        /// </remarks>
        [HttpGet(ApiRoutes.Manager.Categories)]
        public IActionResult GetEventCategories()
        {
            var authResponse = _categoryBusiness.GetEventCategories();

            return Ok(authResponse);
        }

        /// <summary>
        /// Creates a new Category for Events
        /// </summary>
        /// <remarks>
        /// ### Usage:
        /// This request does not use the `Body` for recieving data, instead it uses `FormData` to support file upload.
        /// </remarks>
        [HttpPost(ApiRoutes.Manager.Categories)]
        public IActionResult NewEventCategory([FromForm]NewEventCategoryViewModel request)
        {
            var authResponse = _categoryBusiness.NewEventCategory(request);

            return Ok(authResponse);
        }

        /// <summary>
        /// Updates an existing Category
        /// </summary>
        /// <remarks>
        /// ### Usage:
        /// This request does not use the `Body` for recieving data, instead it uses `FormData` to support file upload.
        /// 
        /// ### Remarks
        /// - Non of the fields is required, and only the provided ones are updated.
        /// - Providing a new Thumbnail will delete the old one from the server storage.
        /// </remarks>
        [HttpPut(ApiRoutes.Manager.Categories)]
        public IActionResult UpdateEventCategory([FromForm]UpdateEventCategoryViewModel request)
        {
            var authResponse = _categoryBusiness.UpdateEventCategory(request);

            return Ok(authResponse);
        }

        /// <summary>
        /// Deletes an existing Category
        /// </summary>
        [HttpPut(ApiRoutes.Manager.Categories)]
        public IActionResult DeleteEventCategory([FromQuery]int categoryId)
        {
            var authResponse = _categoryBusiness.DeleteEventCategory(categoryId);

            return Ok(authResponse);
        }

        /// <summary>
        /// Get a list of events in your region of management
        /// </summary>
        [HttpGet(ApiRoutes.Manager.Events)]
        public async Task<IActionResult> GetEvents()
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var authResponse = await _eventsBusiness.GetEventsForManagerAsync(currentlyLoggedInUserId);

            return Ok(authResponse);
        }

        /// <summary>
        /// Mark an event in your region as Valid
        /// </summary>
        [HttpPut(ApiRoutes.Manager.Events)]
        public async Task<IActionResult> ValidateEvent([FromQuery] int eventId)
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var authResponse = await _eventsBusiness.UpdateEventValidity(currentlyLoggedInUserId, eventId, true);

            return Ok(authResponse);
        }

        /// <summary>
        /// Mark an event in your region as Invalid
        /// </summary>
        [HttpPut(ApiRoutes.Manager.Events)]
        public async Task<IActionResult> InvalidateEvent([FromQuery] int eventId)
        {
            string currentlyLoggedInUserId = User.Claims.Where(x => x.Type == "id").FirstOrDefault()?.Value;

            var authResponse = await _eventsBusiness.UpdateEventValidity(currentlyLoggedInUserId, eventId, false);

            return Ok(authResponse);
        }
    }
}